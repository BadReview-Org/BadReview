using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using FluentValidation.Results;

using BadReview.Api.Data;
using BadReview.Api.Models;
using BadReview.Api.Services;

using BadReview.Shared.DTOs.Response;
using BadReview.Shared.DTOs.Request;

using static BadReview.Api.Mapper.Mapper;
using static BadReview.Api.Services.IUserService;
using static BadReview.Api.Services.IReviewService;
using Microsoft.AspNetCore.Mvc;

namespace BadReview.Api.Endpoints;

public static class UserEndpoints
{
    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/login", LoginUser)
            .WithName("LoginUser")
            .WithTags("Authentication")
            .WithSummary("Log in")
            .WithDescription("Authenticate an user and return JWT tokens");

        app.MapPost("/api/register", RegisterUser)
            .WithName("RegisterUser")
            .WithTags("Authentication")
            .WithSummary("Register user")
            .WithDescription("Create a new user account")
            .Produces<UserTokensDto>(StatusCodes.Status200OK, contentType: "application/json");

        app.MapPost("/api/refresh", RefreshUserTokens)
            .WithName("RefreshTokens")
            .RequireAuthorization("RefreshTokenPolicy")
            .WithTags("Authentication")
            .WithSummary("Refresh tokens")
            .WithDescription("Generate new access tokens using the refresh token");

        // PUT: /api/profile - Update an existing user
        app.MapPut("/api/profile", UpdateUser)
            .RequireAuthorization("AccessTokenPolicy")
            .WithName("UpdateUser")
            .WithTags("User Profile")
            .WithSummary("Update profile")
            .WithDescription("Update the authenticated user's information");

        // DELETE: /api/users/{id} - Delete a user
        app.MapDelete("/api/profile", DeleteUser)
            .RequireAuthorization("AccessTokenPolicy")
            .WithName("DeleteUser")
            .WithTags("User Profile")
            .WithSummary("Delete account")
            .WithDescription("Delete the authenticated user's account");

        // autorizar, traer private dto, paginar
        app.MapGet("/api/profile", GetPrivateProfile)
            .RequireAuthorization("AccessTokenPolicy")
            .WithTags("User Profile")
            .WithSummary("Get private profile")
            .WithDescription("Get the complete information of the authenticated user's profile");

        // GET: /api/users/{id} - Get a user by ID
        app.MapGet("/api/users/{id}", GetPublicProfile)
            .WithName("GetUser")
            .WithTags("User Profile")
            .WithSummary("Get user by ID")
            .WithDescription("Get the public information of a user by their ID");


        app.MapPost("/api/usernameavailable", IsUsernameAvailable)
            .WithTags("Validation")
            .WithSummary("Check username availability")
            .WithDescription("Check if a username is available");

        app.MapPost("/api/emailavailable", IsEmailAvailable)
            .WithTags("Validation")
            .WithSummary("Check email availability")
            .WithDescription("Check if an email is available for registration")
            .Produces<string>(StatusCodes.Status200OK, contentType: "application/json")
            .Produces<string>(StatusCodes.Status409Conflict, contentType: "application/json")
            .ProducesProblem(StatusCodes.Status400BadRequest);
        // GET: /api/users - Get all users (for debugging only)
        //app.MapGet("/api/users", GetUsers).WithName("GetUsers");

        return app;
    }


    private static async Task<IResult> RefreshUserTokens
    (ClaimsPrincipal claims, IUserService userService)
    {
        (UserCode code, UserTokensDto? dto) = await userService.RefreshTokens(claims);

        var response = code switch
        {
            UserCode.OK => Results.Ok(dto),
            UserCode.BADUSERCLAIMS => Results.BadRequest("Can't retrieve user id from JWT claims."),
            UserCode.USERNAMENOTFOUND => Results.NotFound("User's not registered."),
            _ => Results.InternalServerError()
        };

        return response;
    }

    private static async Task<IResult> LoginUser
    (LoginUserRequest req, IUserService userService, IValidator<LoginUserRequest> validator)
    {
        ValidationResult validation = await validator.ValidateAsync(req);
        if (!validation.IsValid) return Results.BadRequest(validation.ToDictionary());

        (UserCode code, UserTokensDto? dto) = await userService.LoginUserAsync(req);

        var response = code switch
        {
            UserCode.OK => Results.Ok(dto),
            UserCode.USERNAMENOTFOUND => Results.NotFound("Username not found"),
            UserCode.PASSDONTMATCH => Results.Unauthorized(),
            _ => Results.InternalServerError()
        };

        return response;
    }

    private static async Task<IResult> RegisterUser
    (CreateUserRequest req, IUserService userService, IValidator<CreateUserRequest> validator)
    {
        ValidationResult validation = await validator.ValidateAsync(req);
        if (!validation.IsValid) return Results.BadRequest(validation.ToDictionary());

        try
        {
            (UserCode code, RegisterUserDto? dto) = await userService.CreateUserAsync(req);

            var response = code switch
            {
                UserCode.OK => Results.Ok(dto),
                _ => Results.InternalServerError()
            };

            return response;
        }
        catch (WritingToDBException ex)
            { return Results.InternalServerError($"Error while persisting data to DB: {ex.Message}"); }
        catch (Exception ex)
            { return Results.InternalServerError($"Unexpected exception: {ex.Message}"); }
    }

    private static async Task<IResult> UpdateUser
    (ClaimsPrincipal claims, UpdateUserRequest req, IUserService userService, ValidatorRules.ICheckAvailables checker)
    {
        (UserCode code, User? user) = await userService.GetPlainUserFromClaims(claims);

        if (code == UserCode.BADUSERCLAIMS)
            return Results.BadRequest("Can't retrieve user id from JWT claims.");
        else if (code == UserCode.USERNAMENOTFOUND || user is null)
            return Results.NotFound("User's not registered.");

        IValidator<UpdateUserRequest> validator = new UpdateUserRequestValidator(checker, user.Username, user.Email);

        ValidationResult validation = await validator.ValidateAsync(req);
        if (!validation.IsValid) return Results.BadRequest(validation.ToDictionary());

        
        try
        {
            (code, BasicUserDto? dto) = await userService.UpdateUserAsync(req, user);

            var response = code switch
            {
                UserCode.OK => Results.Ok(dto),
                _ => Results.InternalServerError()
            };

            return response;
        }
        catch (WritingToDBException ex)
            { return Results.InternalServerError($"Error while persisting data to DB: {ex.Message}"); }
        catch (Exception ex)
            { return Results.InternalServerError($"Unexpected exception: {ex.Message}"); }
    }

    private static async Task<IResult> DeleteUser
    (ClaimsPrincipal claims, IUserService userService)
    {
        (UserCode code, User? user) = await userService.GetPlainUserFromClaims(claims);

        if (code == UserCode.BADUSERCLAIMS)
            return Results.BadRequest("Can't retrieve user id from JWT claims.");
        else if (code == UserCode.USERNAMENOTFOUND || user is null)
            return Results.NotFound("User's not registered.");


        try
        {
            code = await userService.DeleteUserAsync(user);

            var response = code switch
            {
                UserCode.OK => Results.NoContent(),
                _ => Results.InternalServerError()
            };

            return response;
        }
        catch (WritingToDBException ex)
            { return Results.InternalServerError($"Error while persisting data to DB: {ex.Message}"); }
        catch (Exception ex)
            { return Results.InternalServerError($"Unexpected exception: {ex.Message}"); }
    }

    private static async Task<IResult> GetPrivateProfile
    (ClaimsPrincipal claims, [AsParameters] PaginationRequest pag, UserPaginationField? field,
    IUserService userService, IReviewService reviewService)
    {
        string? claimUserId =
            claims.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (claimUserId is null) return Results.BadRequest("Can't retrieve user id from JWT claims.");


        int userId = int.Parse(claimUserId);
        switch (field)
        {
            case null:
                (UserCode code, PrivateUserDto? dto) = await userService.GetUserPrivateData(userId, pag);

                return code switch
                {
                    UserCode.OK => Results.Ok(dto),
                    UserCode.USERNAMENOTFOUND => Results.NotFound($"User with id {userId} not found"),
                    _ => Results.InternalServerError()
                };
            case UserPaginationField.REVIEWS:
                var reviewsPage = await reviewService.GetDetailReviewsAsync(pag, true, GetReviewsOpt.REVIEWS, userId);

                return Results.Ok(reviewsPage);
            case UserPaginationField.FAVORITES:
                var favoritesPage = await reviewService.GetDetailReviewsAsync(pag, true, GetReviewsOpt.FAVORITES, userId);

                return Results.Ok(favoritesPage);
            default:
                return Results.BadRequest("Pagination field is incorrect.");
        }
    }

    private static async Task<IResult> GetPublicProfile
    (int id, [AsParameters] PaginationRequest pag, UserPaginationField? field, IUserService userService, IReviewService reviewService)
    {
        switch (field)
        {
            case null:
                (UserCode code, PublicUserDto? dto) = await userService.GetUserPublicData(id, pag);

                return code switch
                {
                    UserCode.OK => Results.Ok(dto),
                    UserCode.USERNAMENOTFOUND => Results.NotFound($"User with id {id} not found"),
                    _ => Results.InternalServerError()
                };
            case UserPaginationField.REVIEWS:
                var reviewsPage = await reviewService.GetDetailReviewsAsync(pag, true, GetReviewsOpt.REVIEWS, id);

                return Results.Ok(reviewsPage);
            case UserPaginationField.FAVORITES:
                var favoritesPage = await reviewService.GetDetailReviewsAsync(pag, true, GetReviewsOpt.FAVORITES, id);

                return Results.Ok(favoritesPage);
            default:
                return Results.BadRequest("Pagination field is incorrect.");
        }
    }

    private static async Task<IResult> IsUsernameAvailable
    (UserCheckAvailable req, IUserService userService)
    {
        string? username = req.Username;
        if (string.IsNullOrWhiteSpace(username)) return Results.BadRequest();

        bool available = await userService.UsernameAvailable(username);

        return available ? Results.Ok(username) : Results.Conflict(username);
    }

    private static async Task<IResult> IsEmailAvailable
    (UserCheckAvailable req, IUserService userService)
    {
        string? email = req.Email;
        if (string.IsNullOrWhiteSpace(email)) return Results.BadRequest();

        bool exists = await userService.EmailAvailable(email);

        return exists ? Results.Ok(email) : Results.Conflict(email);
    }
}
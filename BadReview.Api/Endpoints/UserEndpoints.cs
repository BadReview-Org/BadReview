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

namespace BadReview.Api.Endpoints;

public static class UserEndpoints
{
    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/login", LoginUser);

        app.MapPost("/api/register", RegisterUser).WithName("RegisterUser");

        // PUT: /api/profile - Actualizar un usuario existente
        app.MapPut("/api/profile", UpdateUser).RequireAuthorization("AccessTokenPolicy").WithName("UpdateUser");

        // DELETE: /api/users/{id} - Eliminar un usuario
        app.MapDelete("/api/profile", DeleteUser).RequireAuthorization("AccessTokenPolicy").WithName("DeleteUser");

        // autorizar, traer private dto, paginar
        app.MapGet("/api/profile", GetPrivateProfile).RequireAuthorization("AccessTokenPolicy");

        // GET: /api/users/{id} - Obtener un usuario por ID
        app.MapGet("/api/users/{id}", GetPublicProfile).WithName("GetUser");

        app.MapPost("/api/refresh", RefreshUserTokens).RequireAuthorization("RefreshTokenPolicy");

        // GET: /api/users - Obtener todos los usuarios (solo para debugging)
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
                UserCode.USERNAMEALREADYEXISTS => Results.Conflict("Username already exists."),
                UserCode.EMAILALREADYEXISTS => Results.Conflict("Email already exists."),
                UserCode.NULLPASSWORD => Results.BadRequest("Didn't receive a password."),
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
    (ClaimsPrincipal claims, CreateUserRequest req, IUserService userService, IValidator<CreateUserRequest> validator)
    {
        ValidationResult validation = await validator.ValidateAsync(req);
        if (!validation.IsValid) return Results.BadRequest(validation.ToDictionary());
        
        try
        {
            (UserCode code, BasicUserDto? dto) = await userService.UpdateUserAsync(claims, req);

            var response = code switch
            {
                UserCode.OK => Results.Ok(dto),
                UserCode.BADUSERCLAIMS => Results.BadRequest("Can't retrieve user id from JWT claims."),
                UserCode.USERNAMENOTFOUND => Results.NotFound("User's not registered."),
                UserCode.USERNAMEALREADYEXISTS => Results.Conflict("Trying to change Username to a one already registered."),
                UserCode.EMAILALREADYEXISTS => Results.Conflict("Trying to change Email to a one already registered."),
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
        try
        {
            UserCode code = await userService.DeleteUserAsync(claims);

            var response = code switch
            {
                UserCode.OK => Results.NoContent(),
                UserCode.BADUSERCLAIMS => Results.BadRequest("Can't retrieve user id from JWT claims."),
                UserCode.USERNAMENOTFOUND => Results.NotFound("User's not registered."),
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
}
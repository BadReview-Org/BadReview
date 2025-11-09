using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

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
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/login", LoginUser);

        app.MapPost("/api/register", RegisterUser).WithName("RegisterUser");

        // PUT: /api/profile - Actualizar un usuario existente
        app.MapPut("/api/profile", UpdateUser).RequireAuthorization().WithName("UpdateUser");

        // DELETE: /api/users/{id} - Eliminar un usuario
        app.MapDelete("/api/profile", DeleteUser).RequireAuthorization().WithName("DeleteUser");

        // autorizar, traer private dto, paginar
        app.MapGet("/api/profile", GetPrivateProfile).RequireAuthorization();

        // GET: /api/users/{id} - Obtener un usuario por ID
        // traer public dto, paginar
        app.MapGet("/api/users/{id}", GetPublicProfile).WithName("GetUser");

        // GET: /api/users - Obtener todos los usuarios (solo para debugging)
        //app.MapGet("/api/users", GetUsers).WithName("GetUsers");
    }


    private static async Task<IResult> LoginUser
    (LoginUserRequest req, IUserService userService)
    {
        (UserCode code, LoginUserDto? dto) = await userService.LoginUserAsync(req);

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
    (RegisterUserRequest req, IUserService userService)
    {
        (UserCode code, RegisterUserDto? dto) = await userService.CreateUserAsync(req);

        var response = code switch
        {
            UserCode.OK => Results.Ok(dto),
            UserCode.USERNAMEALREADYEXISTS => Results.Conflict("Username already exists."),
            UserCode.EMAILALREADYEXISTS => Results.Conflict("Email already exists."),
            _ => Results.InternalServerError()
        };

        return response;
    }

    private static async Task<IResult> UpdateUser
    (ClaimsPrincipal claims, CreateUserRequest req, IUserService userService)
    {
        (UserCode code, BasicUserDto? dto) = await userService.UpdateUserAsync(claims, req);

        var response = code switch
        {
            UserCode.OK => Results.Ok(dto),
            UserCode.BADUSERCLAIMS => Results.BadRequest("Can't retrieve user id from JWT claims."),
            UserCode.USERNAMENOTFOUND => Results.NotFound("User's not registered'."),
            UserCode.USERNAMEALREADYEXISTS => Results.Conflict("Trying to change Username to a one already registered."),
            UserCode.EMAILALREADYEXISTS => Results.Conflict("Trying to change Email to a one already registered."),
            _ => Results.InternalServerError()
        };

        return response;
    }

    private static async Task<IResult> DeleteUser
    (ClaimsPrincipal claims, IUserService userService)
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

    private static async Task<IResult> GetPrivateProfile
    (ClaimsPrincipal user)
    {
        throw new NotImplementedException();
        /*var user = await db.Users
            .Include(u => u.Reviews)
                .ThenInclude(r => r.Game)
            .FirstOrDefaultAsync(u => u.Id == id);
        var userdto = user is not null ? new PublicUserDto(
            user.Id,
            user.Username,
            user.Country,
            user.Reviews.Select(r => new BasicReviewDto(
                r.Id,
                r.Rating,
                r.ReviewText,
                r.StateEnum,
                r.IsFavorite, r.IsReview,
                null,
                new BasicGameDto(
                    r.Game.Id,
                    r.Game.Name,
                    r.Game.Cover?.ImageId,
                    r.Game.Cover?.ImageHeight,
                    r.Game.Cover?.ImageWidth,
                    r.Game.RatingIGDB,
                    r.Game.Total_RatingBadReview,
                    r.Game.Count_RatingBadReview
                ),
                r.Date.UpdatedAt
            )
            ).ToPagedResult(0, 0, 0),
            user.Reviews.Select(r => new BasicReviewDto(
                r.Id,
                r.Rating,
                r.ReviewText,
                r.StateEnum,
                r.IsFavorite, r.IsReview,
                null,
                new BasicGameDto(
                    r.Game.Id,
                    r.Game.Name,
                    r.Game.Cover?.ImageId,
                    r.Game.Cover?.ImageHeight,
                    r.Game.Cover?.ImageWidth,
                    r.Game.RatingIGDB,
                    r.Game.Total_RatingBadReview,
                    r.Game.Count_RatingBadReview
                ),
                r.Date.UpdatedAt
            )
            ).ToPagedResult(0, 0, 0),
            user.Date.UpdatedAt
        ) : null;
        return userdto is not null ? Results.Ok(userdto) : Results.NotFound();*/
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
                var reviewsPage = await reviewService.GetReviewsAsync(pag, GetReviewsOpt.REVIEWS, id);

                return Results.Ok(reviewsPage);
            case UserPaginationField.FAVORITES:
                var favoritesPage = await reviewService.GetReviewsAsync(pag, GetReviewsOpt.FAVORITES, id);

                return Results.Ok(favoritesPage);
            default:
                return Results.BadRequest("Pagination field is incorrect.");
        }
    }

    /*private static async Task<IResult> GetUsers
    (BadReviewContext db)
    {
        var users = await db.Users
            .Include(u => u.Reviews)
                .ThenInclude(r => r.Game)
            .ToListAsync();

        return users.Select(u => new PrivateUserDto(
            u.Id,
            u.Username,
            u.FullName,
            u.Birthday,
            u.Country,
            u.Reviews.Select(r => new DetailReviewDto(
                r.Id,
                r.Rating,
                r.StartDate,
                r.EndDate,
                r.ReviewText,
                r.StateEnum,
                r.IsFavorite,
                null,
                new BasicGameDto(
                    r.Game.Id,
                    r.Game.Name,
                    r.Game.Cover?.ImageId,
                    r.Game.Cover?.ImageHeight,
                    r.Game.Cover?.ImageWidth,
                    r.Game.RatingIGDB,
                    r.Game.Total_RatingBadReview,
                    r.Game.Count_RatingBadReview
                ),
                r.Date.CreatedAt, r.Date.UpdatedAt
            )
            ).ToList(),
            u.Date.CreatedAt, u.Date.UpdatedAt
        ));
    }*/
}
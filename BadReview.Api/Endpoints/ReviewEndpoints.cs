using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using BadReview.Api.Data;
using BadReview.Api.Models;

using BadReview.Shared.DTOs.Request;
using BadReview.Shared.DTOs.Response;
using BadReview.Api.Services;
using Microsoft.AspNetCore.Mvc;

using static BadReview.Api.Services.IReviewService;

namespace BadReview.Api.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        // GET: /api/reviews - Obtener todas las reseñas
        app.MapGet("/api/reviews", GetReviews);

        // GET: /api/reviews/{id} - Obtener una reseña por ID
        app.MapGet("/api/reviews/{id}", GetReviewById);

        //PUT: /api/reviews/{id} - Actualizar una reseña por ID
        app.MapPut("/api/reviews/{id}", UpdateReviewWithId).RequireAuthorization();

        // DELETE: /api/reviews/{id} - Eliminar una reseña por ID
        app.MapDelete("/api/reviews/{id}", DeleteReviewWithId).RequireAuthorization();

        // POST: /api/reviews - Crear una nueva reseña
        app.MapPost("/api/reviews", CreateReview).RequireAuthorization().WithName("ReviewEndpoints");
    }

    private static async Task<IResult> GetReviews
    ([AsParameters] PaginationRequest pag, IReviewService reviewService)
    {
        pag.SetDefaults();

        var reviewPage = await reviewService.GetBasicReviewsAsync(pag);

        return Results.Ok(reviewPage);
    }

    private static async Task<IResult> GetReviewById
    (int id, IReviewService reviewService)
    {
        var reviewDto = await reviewService.GetReviewByIdAsync(id);

        var response = reviewDto is null ?
            Results.NotFound($"No review with id {id}") : Results.Ok(reviewDto);

        return response;
    }

    private static async Task<IResult> UpdateReviewWithId
    (int id, ClaimsPrincipal user, CreateReviewRequest updatedReview, IReviewService reviewService)
    {
        string? claimUserId = user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (claimUserId is null) return Results.Forbid();

        var (code, reviewDto) = await reviewService.UpdateReviewAsync(id, int.Parse(claimUserId), updatedReview);

        IResult response = code switch
        {
            ReviewCode.REVIEWNOTFOUND => Results.NotFound($"No review matching the id {id}"),
            ReviewCode.USERNOTMATCH => Results.BadRequest($"Review does not match with the user credentials"),
            ReviewCode.OK => Results.Ok(reviewDto),
            _ => Results.InternalServerError()
        };

        return response;
    }

    private static async Task<IResult> DeleteReviewWithId
    (int id, ClaimsPrincipal user, IReviewService reviewService)
    {
        string? claimUserId = user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (claimUserId is null) return Results.Forbid();

        var code = await reviewService.DeleteReviewAsync(id, int.Parse(claimUserId));

        IResult response = code switch
        {
            ReviewCode.REVIEWNOTFOUND => Results.NotFound($"No review matching the id {id}"),
            ReviewCode.USERNOTMATCH => Results.BadRequest($"Review does not match with the user credentials"),
            ReviewCode.OK => Results.NoContent(),
            _ => Results.InternalServerError()
        };

        return response;
    }

    private static async Task<IResult> CreateReview
    (CreateReviewRequest newReview, ClaimsPrincipal user, IReviewService reviewService, IUserService userService)
    {
        string? claimUserId = user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (claimUserId is null) return Results.Forbid();

        User? userDb = await userService.GetUserByIdAsync(int.Parse(claimUserId));
        if (userDb is null) return Results.NotFound($"User with id {claimUserId} not found in the db.");


        var (code, reviewDto) = await reviewService.CreateReviewAsync(newReview, userDb);

        IResult response = code switch
        {
            ReviewCode.GAMENOTFOUND => Results.NotFound($"No game in the db matching the id {newReview.GameId}"),
            ReviewCode.USERALREADYHASREVIEW => Results.Conflict($"User already has a review with game id {newReview.GameId}"),
            ReviewCode.OK => Results.Ok(reviewDto),
            _ => Results.InternalServerError()
        };

        return response;
    }
}

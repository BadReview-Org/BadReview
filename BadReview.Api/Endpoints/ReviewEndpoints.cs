using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using BadReview.Api.Data;
using BadReview.Api.Models;

using BadReview.Shared.DTOs.Request;
using BadReview.Shared.DTOs.Response;
using BadReview.Api.Services;

namespace BadReview.Api.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        // GET: /api/reviews - Obtener todas las reseñas
        app.MapGet("/api/reviews", async (PaginationRequest pag, IReviewService reviewService) =>
        {
            var reviewPage = await reviewService.GetReviewsAsync(pag);

            return Results.Ok(reviewPage);
        });

        // GET: /api/reviews/{id} - Obtener una reseña por ID
        app.MapGet("/api/reviews/{id}", async (int id, IReviewService reviewService) =>
        {
            var reviewDto = await reviewService.GetReviewByIdAsync(id);

            var response = reviewDto is null ?
                Results.NotFound($"No review with id {id}") : Results.Ok(reviewDto);

            return response;
        });

        //PUT: /api/reviews/{id} - Actualizar una reseña por ID
        app.MapPut("/api/reviews/{id}", async (int id, ClaimsPrincipal user, CreateReviewRequest updatedReview, IReviewService reviewService) =>
        {
            string? claimUserId = user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (claimUserId is null) return Results.Forbid();

            var reviewDto = await reviewService.UpdateReviewAsync(id, int.Parse(claimUserId), updatedReview);

            var response = reviewDto is null ?
                Results.Problem() : Results.Ok(reviewDto);

            return response;
        })
        .RequireAuthorization();

        // DELETE: /api/reviews/{id} - Eliminar una reseña por ID
        app.MapDelete("/api/reviews/{id}", async (int id, ClaimsPrincipal user, IReviewService reviewService) =>
        {
            string? claimUserId = user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (claimUserId is null) return Results.Forbid();

            var deleted = await reviewService.DeleteReviewAsync(id, int.Parse(claimUserId));

            return deleted ? Results.NoContent() : Results.Problem();
        })
        .RequireAuthorization();

        // POST: /api/reviews - Crear una nueva reseña
        app.MapPost("/api/reviews", async (CreateReviewRequest newReview, ClaimsPrincipal user, IReviewService reviewService, IUserService userService) =>
        {
            string? claimUserId = user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (claimUserId is null) return Results.Forbid();

            User? userDb = await userService.GetUserByIdAsync(int.Parse(claimUserId));
            if (userDb is null) return Results.NotFound($"User with id {claimUserId} not found in the db.");


            DetailReviewDto? reviewDto = await reviewService.CreateReviewAsync(newReview, userDb);

            var response = reviewDto is null ?
                Results.Problem() : Results.Created($"/api/reviews/{reviewDto.Id}", reviewDto);

            return response;
        })
        .RequireAuthorization()
        .WithName("ReviewEndpoints");
    }
}

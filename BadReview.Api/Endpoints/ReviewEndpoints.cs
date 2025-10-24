using BadReview.Api.Data;
using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BadReview.Api.Endpoints;
public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        // GET: /api/reviews - Obtener todas las reseñas
        app.MapGet("/api/reviews", async (BadReviewContext db) =>
        {
            var reviews = await db.Reviews
                .Include(r => r.User)
                .ToListAsync();

            return Results.Ok(reviews);
        });
        // POST: /api/reviews - Crear una nueva reseña
        _ = app.MapPost("/api/reviews", async (Review review, HttpContext context, BadReviewContext db) =>
        {

            if (!context.Request.Headers.TryGetValue("userId", out var userIdHeader))
            {
                return Results.BadRequest(new { error = "userId header is required" });
            }

            if (!int.TryParse(userIdHeader.ToString(), out var userId))
            {
                return Results.BadRequest(new { error = "userId must be a valid integer" });
            }
            if (!context.Request.Query.ContainsKey("gameId"))
            {
                return Results.BadRequest(new { error = "Game parameters are required" });
            }
            var gameIdString = context.Request.Query["gameId"].ToString();
            if (!int.TryParse(gameIdString, out var gameId))
            {
                return Results.BadRequest(new { error = "gameId must be a valid integer" });
            }
            var gameExists = await db.Games.AnyAsync(g => g.Id == gameId);
            if(!gameExists)
            {
                return Results.NotFound(new { error = $"Game with id {gameId} not found" });
            }   
            // Verificar que el usuario existe en la base de datos
            var userExists = await db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return Results.NotFound(new { error = $"User with id {userId} not found" });
            }

            // Asignar el userId a la review
            review.UserId = userId;
            review.GameId = gameId;

            db.Reviews.Add(review);
            await db.SaveChangesAsync();
            return Results.Created($"/api/reviews/{review.Id}", review);
        });
    }
}
using BadReview.Api.Data;
using BadReview.Api.Models;
using BadReview.Api.DTOs.Request;
using Microsoft.EntityFrameworkCore;
using BadReview.Api.DTOs.Response;

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
                
            if (reviews != null)
            {
                return Results.Ok(reviews.Select(r => new DetailReviewDto(
                    r.Id,
                    r.Rating,
                    r.StartDate,
                    r.EndDate,
                    r.ReviewText,
                    r.StateEnum,
                    r.IsFavorite,
                    new BasicUserDto(
                        r.User.Id,
                        r.User.Username,
                        r.User.FullName
                    ),
                    null!
                )).ToList());

            }
            return Results.NotFound();
        });




        // POST: /api/reviews - Crear una nueva reseña
        app.MapPost("/api/reviews", async (CreateReviewRequest review, HttpContext context, BadReviewContext db) =>
        {

            if (!context.Request.Headers.TryGetValue("userId", out var userIdHeader))
            {
                return Results.BadRequest(new { error = "userId header is required" });
            }

            if (!int.TryParse(userIdHeader.ToString(), out var userId))
            {
                return Results.BadRequest(new { error = "userId must be a valid integer" });
            }
            var gameId = review.GameId;
            var gameExists = await db.Games.AnyAsync(g => g.Id == gameId);
            if (!gameExists)
            {
                return Results.NotFound(new { error = $"Game with id {gameId} not found" });
            }
            // Verificar que el usuario existe en la base de datos
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                return Results.NotFound(new { error = $"User with id {userId} not found" });
            }
            var reviewdb = new Review
            {
                Rating = review.Rating,
                StartDate = review.StartDate,
                EndDate = review.EndDate,
                ReviewText = review.ReviewText,
                StateEnum = review.StateEnum,
                IsFavorite = review.IsFavorite,
                UserId = userId,
                GameId = gameId
            };
            db.Reviews.Add(reviewdb);
            await db.SaveChangesAsync();
            var reviewdto = new DetailReviewDto
            (
                reviewdb.Id,
                reviewdb.Rating,
                reviewdb.StartDate,
                reviewdb.EndDate,
                reviewdb.ReviewText,
                reviewdb.StateEnum,
                reviewdb.IsFavorite,
                new BasicUserDto(
                    userId,
                    user.Username,
                    user.FullName
                ),
                null!
            );
            return Results.Created($"/api/reviews/{reviewdto.Id}", reviewdto);
        });
    }
}
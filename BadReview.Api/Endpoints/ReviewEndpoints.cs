using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using BadReview.Api.Data;
using BadReview.Api.Models;

using BadReview.Shared.DTOs.Request;
using BadReview.Shared.DTOs.Response;

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
        // GET: /api/reviews/{id} - Obtener una reseña por ID
        app.MapGet("/api/reviews/{id}", async (int id, BadReviewContext db) =>
        {
            var review = await db.Reviews
                .Include(r => r.User)
                .Include(r => r.Game)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review != null)
            {
                var reviewdto = new DetailReviewDto
                (
                    review.Id,
                    review.Rating,
                    review.StartDate,
                    review.EndDate,
                    review.ReviewText,
                    review.StateEnum,
                    review.IsFavorite,
                    new BasicUserDto(
                        review.User.Id,
                        review.User.Username,
                        review.User.FullName
                    ),
                    new BasicGameDto(
                        review.Game.Id,
                        review.Game.Name,
                        review.Game.Cover,
                        review.Game.RatingIGDB,
                        review.Game.Total_RatingBadReview,
                        review.Game.Count_RatingBadReview
                    )
                );

                return Results.Ok(reviewdto);
            }
            return Results.NotFound();
        });





        //PUT: /api/reviews/{id} - Actualizar una reseña por ID
        app.MapPut("/api/reviews/{id}", async (int id, ClaimsPrincipal user, CreateReviewRequest updatedReview, BadReviewContext db) =>
        {
            var review = await db.Reviews.Include(r => r.User)
                                         .Include(r => r.Game)
                                         .FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
            {
                return Results.NotFound(new { error = $"Review with id {id} not found" });
            }
            if (user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value != review.UserId.ToString())
            {
                return Results.Forbid();
            }
            review.Game.Total_RatingBadReview -= review.Rating ?? 0;
            review.Game.Total_RatingBadReview += updatedReview.Rating ?? 0;

            review.Rating = updatedReview.Rating;
            review.StartDate = updatedReview.StartDate;
            review.EndDate = updatedReview.EndDate;
            review.ReviewText = updatedReview.ReviewText;
            review.StateEnum = updatedReview.StateEnum;
            review.IsFavorite = updatedReview.IsFavorite;

            await db.SaveChangesAsync();

            var reviewdto = new DetailReviewDto
            (
                review.Id,
                review.Rating,
                review.StartDate,
                review.EndDate,
                review.ReviewText,
                review.StateEnum,
                review.IsFavorite,
                new BasicUserDto(
                    review.User.Id,
                    review.User.Username,
                    review.User.FullName
                ),
                new BasicGameDto(
                    review.Game.Id,
                    review.Game.Name,
                    review.Game.Cover,
                    review.Game.RatingIGDB,
                    review.Game.Total_RatingBadReview,
                    review.Game.Count_RatingBadReview
                )
            );

            return Results.Ok(reviewdto);
        })
        .RequireAuthorization();


        app.MapDelete("/api/reviews/{id}", async (int id, ClaimsPrincipal user, BadReviewContext db) =>
        {
            var review = await db.Reviews.Include(r => r.Game)
                                         .FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
            {
                return Results.NotFound(new { error = $"Review with id {id} not found" });
            }
            if (user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value != review.UserId.ToString())
            {
                return Results.Forbid();
            }
            review.Game.Total_RatingBadReview -= review.Rating ?? 0;
            review.Game.Count_RatingBadReview--;
            
            db.Reviews.Remove(review);
            await db.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization();
        
        // POST: /api/reviews - Crear una nueva reseña

        app.MapPost("/api/reviews", async (CreateReviewRequest review, ClaimsPrincipal user, BadReviewContext db) =>
        {
            var userId = user.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            // Verificar que el usuario existe en la base de datos
            if(userId == null)
            {
                return Results.Unauthorized();
            }
            var userdb = await db.Users.Include(u => u.Reviews)
                                       .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (userdb == null)
            {
                return Results.NotFound(new { error = $"User with id {userId} not found" });
            }
            var gameId = review.GameId;
            var game = await db.Games.FindAsync(gameId);
            if (game == null)
            {
                return Results.NotFound(new { error = $"Game with id {gameId} not found" });
            }
            if (userdb.Reviews.Select(r => r.GameId).Contains(gameId))
            {
                return Results.Conflict(new { error = $"User has already reviewed game with id {gameId}" });
            }
            
            game.Total_RatingBadReview += review.Rating ?? 0;
            game.Count_RatingBadReview++;

            var reviewdb = new Review
            {
                Rating = review.Rating,
                StartDate = review.StartDate,
                EndDate = review.EndDate,
                ReviewText = review.ReviewText,
                StateEnum = review.StateEnum,
                IsFavorite = review.IsFavorite,
                UserId = userdb.Id,
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
                    userdb.Id,
                    userdb.Username,
                    userdb.FullName
                ),
                new BasicGameDto(
                    game.Id,
                    game.Name,
                    game.Cover,
                    game.RatingIGDB++,
                    game.Total_RatingBadReview,
                    game.Count_RatingBadReview
                )
            );
            return Results.Created($"/api/reviews/{reviewdto.Id}", reviewdto);
        })
        .WithName("ReviewEndpoints")
        .RequireAuthorization();
    }
}

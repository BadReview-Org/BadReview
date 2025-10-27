using BadReview.Api.Data;
using BadReview.Api.Models;
using BadReview.Api.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using BadReview.Api.DTOs.Request;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BadReview.Api.Services;

namespace BadReview.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {


            // GET: /api/users - Obtener todos los usuarios
            app.MapGet("/api/users", async (BadReviewContext db) =>
            {
                var users = await db.Users
                    .Include(u => u.Reviews)
                        .ThenInclude(r => r.Game)
                    .ToListAsync();

                return users.Select(u => new UserDto(
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
                            r.Game.Cover,
                            r.Game.RatingIGDB,
                            r.Game.RatingBadReview
                        )
                    )).ToList()
                ));
            })
            .WithName("GetUsers");

            app.MapPost("/login", async (LoginUserRequest req, AuthService auth, BadReviewContext db) =>
            {
                var hashedPass = await db.Users
                    .Where(u => u.Username == req.Username)
                    .Select(u => u.Password)
                    .FirstOrDefaultAsync();
                    
                if (string.IsNullOrEmpty(hashedPass))
                    return Results.NotFound();

                var isValid = auth.VerifyPassword(req.Username, req.Password, hashedPass);

                if (!isValid)
                    return Results.Unauthorized();

                var token = auth.GenerateToken(req.Username);
                return Results.Ok(new { token });
            });
                
            app.MapGet("/profile", [Authorize] (ClaimsPrincipal user) =>
            {
                var username = user.Identity?.Name ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
                return Results.Ok(new { message = $"Hola {username}" });
            });

            // GET: /api/users/{id} - Obtener un usuario por ID
            app.MapGet("/api/users/{id}", async (int id, BadReviewContext db) =>
            {
                var user = await db.Users
                    .Include(u => u.Reviews)
                        .ThenInclude(r => r.Game)
                    .FirstOrDefaultAsync(u => u.Id == id);
                var userdto = user is not null ? new UserDto(
                    user.Id,
                    user.Username,
                    user.FullName,
                    user.Birthday,
                    user.Country,
                    user.Reviews.Select(r => new DetailReviewDto(
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
                            r.Game.Cover,
                            r.Game.RatingIGDB,
                            r.Game.RatingBadReview
                        )
                    )).ToList()
                ) : null;
                return userdto is not null ? Results.Ok(userdto) : Results.NotFound();
            })
            .WithName("GetUser");

            app.MapPost("/register", async (BadReviewContext db, RegisterUserRequest req, AuthService auth) =>
            {
                // Validar que Username sea único
                if (await db.Users.AnyAsync(u => u.Username == req.Username))
                {
                    return Results.Conflict(new { error = "Already exists: Username" });
                }
                // Validar que Email sea único
                if (await db.Users.AnyAsync(u => u.Email == req.Email))
                {
                    return Results.Conflict(new { error = "Already exists: Email" });
                }
                var hashedPassword = auth.HashPassword(req.Username, req.Password);
                var newUser = new User
                {
                    Username = req.Username,
                    Email = req.Email,
                    Password = hashedPassword,
                    FullName = req.FullName
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();
                var userdto = new BasicUserDto(
                    newUser.Id,
                    newUser.Username,
                    newUser.FullName
                );
                var token = auth.GenerateToken(req.Username);

                return Results.Ok(new { user = userdto, token });
            })
            .WithName("RegisterUser");

            // PUT: /api/users/{id} - Actualizar un usuario existente
            app.MapPut("/api/users/{id}", async (int id, CreateUserRequest user, BadReviewContext db) =>
            {

                var existingUser = await db.Users.FindAsync(id);
                if (existingUser is null)
                {
                    return Results.NotFound();
                }

                // Validar que Username sea único (excluyendo el usuario actual)
                if (await db.Users.AnyAsync(u => u.Username == user.Username && u.Id != id))
                {
                    return Results.BadRequest(new { error = "Username already exists" });
                }

                // Validar que Email sea único (excluyendo el usuario actual)
                if (await db.Users.AnyAsync(u => u.Email == user.Email && u.Id != id))
                {
                    return Results.BadRequest(new { error = "Email already exists" });
                }

                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.FullName = user.FullName;
                existingUser.Birthday = user.Birthday;
                existingUser.Country = user.Country;

                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("UpdateUser");

            // DELETE: /api/users/{id} - Eliminar un usuario
            app.MapDelete("/api/users/{id}", async (int id, BadReviewContext db) =>
            {
                var user = await db.Users.FindAsync(id);
                if (user is null)
                {
                    return Results.NotFound();
                }

                db.Users.Remove(user);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("DeleteUser");
        }
    }
}

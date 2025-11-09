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

namespace BadReview.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // GET: /api/users - Obtener todos los usuarios (solo para debugging)
        /*app.MapGet("/api/users", async (BadReviewContext db) =>
        {
            var users = await db.Users
                .Include(u => u.Reviews)
                    .ThenInclude(r => r.Game)
                .ToListAsync();

            return users.Select(u => new DetailUserDto(
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
        })
        .WithName("GetUsers");*/

        app.MapPost("/api/login", async (LoginUserRequest req, IUserService userService) =>
        {
            (UserCode code, string? token) = await userService.LoginUserAsync(req);

            var response = code switch
            {
                UserCode.OK => Results.Ok(new LoginUserDto(token!)),
                UserCode.USERNAMENOTFOUND => Results.NotFound("Username not found"),
                UserCode.PASSDONTMATCH => Results.Unauthorized(),
                _ => Results.InternalServerError()
            };

            return response;
        });

        // autorizar, traer private dto, paginar
        app.MapGet("/api/profile", [Authorize] (ClaimsPrincipal user) =>
        {
            var username = user.Identity?.Name ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Results.Ok(new { message = $"Hola {username}" });
        });

        // GET: /api/users/{id} - Obtener un usuario por ID
        // traer public dto, paginar
        app.MapGet("/api/users/{id}", async (int id, BadReviewContext db) =>
        {
            var user = await db.Users
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
            return userdto is not null ? Results.Ok(userdto) : Results.NotFound();
        })
        .WithName("GetUser");

        app.MapPost("/api/register", async (BadReviewContext db, RegisterUserRequest req, IAuthService auth) =>
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
            var userDto = new BasicUserDto(
                newUser.Id,
                newUser.Username
            );

            var token = auth.GenerateToken(req.Username, newUser.Id);

            return Results.Ok(new RegisterUserDto(userDto, token));
        })
        .WithName("RegisterUser");

        // PUT: /api/users/{id} - Actualizar un usuario existente
        //cambiar a PUT /api/profile
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
        app.MapDelete("/api/profile", async (ClaimsPrincipal user, BadReviewContext db) =>
        {
            var username = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var existingUser = await db.Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
            if (existingUser is null)
            {
                return Results.NotFound();
            }

            db.Users.Remove(existingUser);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteUser");
    }
}
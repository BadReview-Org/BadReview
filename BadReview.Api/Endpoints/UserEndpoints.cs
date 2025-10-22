using BadReview.Api.Data;
using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BadReview.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            // GET: /api/users - Obtener todos los usuarios
            app.MapGet("/api/users", async (BadReviewContext db) =>
            {
                return await db.Users.ToListAsync();
            })
            .WithName("GetUsers");

            // GET: /api/users/{id} - Obtener un usuario por ID
            app.MapGet("/api/users/{id}", async (int id, BadReviewContext db) =>
            {
                var user = await db.Users.FindAsync(id);
                return user is not null ? Results.Ok(user) : Results.NotFound();
            })
            .WithName("GetUser");

            // POST: /api/users - Crear un nuevo usuario
            app.MapPost("/api/users", async (User user, BadReviewContext db) =>
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
                return Results.Created($"/api/users/{user.Id}", user);
            })
            .WithName("CreateUser");

            // PUT: /api/users/{id} - Actualizar un usuario existente
            app.MapPut("/api/users/{id}", async (int id, User user, BadReviewContext db) =>
            {
                if (id != user.Id)
                {
                    return Results.BadRequest();
                }

                var existingUser = await db.Users.FindAsync(id);
                if (existingUser is null)
                {
                    return Results.NotFound();
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

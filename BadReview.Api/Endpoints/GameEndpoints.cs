using BadReview.Api.Data;
using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BadReview.Api.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        // GET: /api/games - Obtener todos los juegos
        app.MapGet("/api/games", async (BadReviewContext db) =>
        {
            var games = await db.Games
                .Include(g => g.GameGenres)
                    .ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers)
                    .ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePlatforms)
                    .ThenInclude(gp => gp.Platform)
                .ToListAsync();

            return Results.Ok(games);
        });

        // GET: /api/games/{id} - Obtener un juego por ID
        app.MapGet("/api/games/{id}", async (int id, BadReviewContext db, IConfiguration config) =>
        {
            // Primero buscar en la base de datos local
            var game = await db.Games
                .Include(g => g.GameGenres)
                    .ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers)
                    .ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePlatforms)
                    .ThenInclude(gp => gp.Platform)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game != null)
            {
                return Results.Ok(game);
            }

            // Si no está en la BD, buscar en IGDB
            try
            {
                var clientId = config["IGDB:ClientId"];
                var accessToken = config["IGDB:AccessToken"];
                //Verificamos las credenciales
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(accessToken))
                {
                    return Results.Problem("IGDB credentials not configured");
                }
                //Completamos el request con los headers y body necesarios
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Client-ID", clientId);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var body = $"fields id, name, cover.url, first_release_date, summary, rating, videos.video_id; where id = {id};";
                var content = new StringContent(body, Encoding.UTF8, "text/plain");

                var response = await client.PostAsync("https://api.igdb.com/v4/games", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var igdbGames = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent);

                    if (igdbGames != null && igdbGames.Count > 0)
                    {
                        var igdbGame = igdbGames[0];
                        
                        // Crear un objeto Game con los datos de IGDB manteniendo el mismo ID
                        var newGame = new Game
                        {
                            Id = igdbGame.GetProperty("id").GetInt32(),
                            Name = igdbGame.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                            Cover = igdbGame.TryGetProperty("cover", out var cover) && cover.TryGetProperty("url", out var url) 
                                ? url.GetString() ?? "" : "",
                            Date = igdbGame.TryGetProperty("first_release_date", out var date) 
                                ? DateTimeOffset.FromUnixTimeSeconds(date.GetInt64()).DateTime : DateTime.MinValue,
                            Summary = igdbGame.TryGetProperty("summary", out var summary) ? summary.GetString() ?? "" : "",
                            RatingIGDB = igdbGame.TryGetProperty("rating", out var rating) ? rating.GetDouble() : 0,
                            RatingBadReview = 0,
                            Video = igdbGame.TryGetProperty("videos", out var videos) && videos.GetArrayLength() > 0
                                ? videos[0].GetProperty("video_id").GetString() ?? "" : ""
                        };

                        // Guardar en la base de datos para cachear
                        db.Games.Add(newGame);
                        await db.SaveChangesAsync();

                        return Results.Ok(newGame);
                    }
                }

                return Results.NotFound(new { message = "Game not found in IGDB" });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error fetching from IGDB: {ex.Message}");
            }
        });
    }
}
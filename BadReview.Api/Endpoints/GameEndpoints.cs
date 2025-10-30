using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using BadReview.Api.Models;
using BadReview.Api.Data;
using BadReview.Api.Services;

using BadReview.Shared.DTOs.Request;
using BadReview.Shared.DTOs.Response;
using BadReview.Shared.DTOs.External;
using BadReview.Shared.Utils;

namespace BadReview.Api.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        // GET: /api/games - Obtener todos los juegos
        app.MapGet("/api/games", async ([AsParameters] SelectGamesRequest query, BadReviewContext db, IGDBClient igdb) =>
        {
            /*var games = await db.Games
                .Include(g => g.GameGenres)
                    .ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers)
                    .ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePlatforms)
                    .ThenInclude(gp => gp.Platform)
                .ToListAsync();

            return Results.Ok(games);*/

            query.SetDefaults();
            object? response;
            switch (query.Detail)
            {
                case IGDBFieldsEnum.BASE:
                    response = await igdb.GetGamesAsync<BasicGameIgdbDto>(query);
                    break;
                case IGDBFieldsEnum.DETAIL:
                    response = await igdb.GetGamesAsync<DetailGameIgdbDto>(query);
                    break;
                default:
                    response = null;
                    break;
            }

            response = response is null ? Results.InternalServerError() : Results.Ok(response);
            return response;
        });

        // GET: /api/games/{id} - Obtener un juego por ID
        app.MapGet("/api/games/{id}", async (int id, BadReviewContext db, IGDBClient igdb) =>
        {

            DetailGameDto? gameDB = await db.Games
                .Where(g => g.Id == id)
                .Select(g => new DetailGameDto(
                    g.Id,
                    g.Name,
                    null, null, null, 0, 0, null,
                    new List<DetailReviewDto>(),
                    g.GameGenres.Select(gg => new GenreDto(gg.GenreId, gg.Genre.Name)).ToList(),
                    new List<DeveloperDto>(),
                    new List<PlatformDto>()))
                .FirstOrDefaultAsync();

            /*IResult res =
                game is null ?
                    Results.NotFound("Game's not registered") :
                    Results.Ok(game);

            return res;*/

            if (gameDB is null)
            {
                var query = new SelectGamesRequest { Filters = $"id = {id}", Detail = IGDBFieldsEnum.BASE };
                query.SetDefaults();
                List<BasicGameIgdbDto>? l = await igdb.GetGamesAsync<BasicGameIgdbDto>(query);

                if (l is not null)
                {
                    BasicGameIgdbDto g = l.First();
                    Game game = new Game { Id = g.Id, Name = g.Name, Cover = g.Cover?.Url };
                    await db.Games.AddAsync(game);
                    await db.SaveChangesAsync();

                    return Results.Ok();
                }

            }
            else
            {
                return Results.Ok(gameDB);
            }

            return Results.InternalServerError();

            // Primero buscar en la base de datos local
            /*var game = await db.Games
                .Include(g => g.GameGenres)
                    .ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers)
                    .ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePlatforms)
                    .ThenInclude(gp => gp.Platform)
                .Include(g => g.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game != null)
            {
                var gameDto = new DetailGameDto(
                    game.Id,
                    game.Name,
                    game.Cover,
                    game.Date,
                    game.Summary,
                    game.RatingIGDB,
                    game.RatingBadReview,
                    game.Video,
                    game.Reviews.Select(r => new DetailReviewDto(
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
                    )).ToList(),
                    game.GameGenres.Select(gg => new GenreDto(
                        gg.Genre.Id,
                        gg.Genre.Name
                    )).ToList(),
                    game.GameDevelopers.Select(gd => new DeveloperDto(
                        gd.Developer.Id,
                        gd.Developer.Name
                    )).ToList(),
                    game.GamePlatforms.Select(gp => new PlatformDto(
                        gp.Platform.Id,
                        gp.Platform.Name
                    )).ToList()
                );

                return Results.Ok(gameDto);
            }
            return Results.NotFound();*/
            /*
            // Si no está en la BD, buscar en IGDB
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Client-ID", clientId);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var body = $"fields id, name, cover.url, first_release_date, summary, rating, videos.video_id; where id = {id};";
                var content = new StringContent(body, Encoding.UTF8, "text/plain");

                var response = await client.PostAsync("https://api.igdb.com/v4/games", content);
                string[] fields = ["id", "name", "cover.url", "first_release_date", "summary", "rating", "videos.video_id"];
                var options = new IGDBQueryOptions { Id = id, Fields =  fields};
                var response = await igdb.GetGamesAsync(options);
                
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
            */
        });
    }
}
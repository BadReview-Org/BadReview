# if false

using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using BadReview.Api.Models;
using BadReview.Api.Data;
using BadReview.Api.Services;

using static BadReview.Api.Mapper.Mapper;

using BadReview.Shared.DTOs.Request;
using BadReview.Shared.DTOs.Response;
using BadReview.Shared.DTOs.External;
using BadReview.Shared.Utils;

namespace BadReview.Api.Endpoints;


public static class PlatformEndpoints
{
    public static void MapPlatformEndpoints(this WebApplication app)
    {
        // GET: /api/platforms - Obtener todos las plataformas
        app.MapGet("/api/platforms", async ([AsParameters] IgdbRequest query, BadReviewContext db, IGDBClient igdb) =>
        {
            var platformsIgdb = await igdb.GetPlatformsAsync<>(query);

            List<GenreDto>? genreList = genresIgdb?.Select(gen => CreateGenreDto(gen)).ToList();

            var response = Results.Ok(genreList);

            return response;
        });

        // GET: /api/platforms/{id} - Obtener una plataforma por ID
        app.MapGet("/api/platforms/{id}", async (int id, BadReviewContext db, IGDBClient igdb) =>
        {
            DetailGameDto? gameDB = await db.Games
                .Where(g => g.Id == id)
                .GameToDetailDto()
                .FirstOrDefaultAsync();

            if (gameDB is not null) Console.WriteLine($"Fetching game: {gameDB.Name}, from DB");
            if (gameDB is not null) return Results.Ok(gameDB);
            else
            {
                var query = new SelectGamesRequest { Filters = $"id = {id}", Detail = IGDBFieldsEnum.DETAIL };
                query.SetDefaults();

                DetailGameIgdbDto? gameIGDB = (await igdb.GetGamesAsync<DetailGameIgdbDto>(query, "games"))?.FirstOrDefault();

                if (gameIGDB is null) return Results.NotFound(id);
                else
                {
                    if (gameIGDB.Genres is not null)
                    {
                        var genreIds = gameIGDB.Genres.Select(g => g.Id).ToHashSet();
                        var existingIds = await db.Genres
                            .Where(g => genreIds.Contains(g.Id))
                            .Select(g => g.Id)
                            .ToHashSetAsync();

                        var newGenres = gameIGDB.Genres
                            .Where(g => !existingIds.Contains(g.Id))
                            .Select(g => CreateGenreEntity(g))
                            .ToList();

                        if (newGenres.Count != 0) db.Genres.AddRange(newGenres);
                    }

                    if (gameIGDB.Platforms is not null)
                    {
                        var platformIds = gameIGDB.Platforms.Select(p => p.Id).ToHashSet();

                        var existingIds = await db.Platforms
                            .Where(p => platformIds.Contains(p.Id))
                            .Select(p => p.Id)
                            .ToHashSetAsync();

                        var newPlatforms = gameIGDB.Platforms
                            .Where(p => !existingIds.Contains(p.Id))
                            .Select(p => CreatePlatformEntity(p))
                            .ToList();

                        if (newPlatforms.Count != 0) db.Platforms.AddRange(newPlatforms);
                    }

                    if (gameIGDB.Involved_Companies is not null)
                    {
                        var devs = gameIGDB.Involved_Companies.Where(c => c.Developer).Select(c => c.Company).ToList();
                        var devIds = devs.Select(d => d.Id).ToHashSet();

                        var existingIds = await db.Developers
                            .Where(d => devIds.Contains(d.Id))
                            .Select(d => d.Id)
                            .ToHashSetAsync();

                        var newDevs = devs
                            .Where(d => !existingIds.Contains(d.Id))
                            .Select(d => CreateDeveloperEntity(d))
                            .ToList();

                        if (newDevs.Count != 0) db.Developers.AddRange(newDevs);
                    }

                    Console.WriteLine($"Saving game {gameIGDB.Name} from IGDB into database");
                    //mapear a Game y persistir
                    var newGame = CreateGameEntity(gameIGDB);

                    db.Games.Add(newGame);
                    await db.SaveChangesAsync();
                    return Results.Ok(CreateDetailGameDto(gameIGDB));
                }
            }
        });
    }
}

#endif
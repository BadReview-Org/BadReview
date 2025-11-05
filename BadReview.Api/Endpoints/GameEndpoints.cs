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
using System.ComponentModel;

namespace BadReview.Api.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        // GET: /api/games - Obtener todos los juegos
        app.MapGet("/api/games", async ([AsParameters] IgdbRequest query, IGDBGameDetail? detail, BadReviewContext db, IGDBClient igdb) =>
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

            detail ??= IGDBCONSTANTS.DEF_GAMEDETAIL;
            query.SetDefaults();

            object? data;
            bool dataNotEmpty = false;
            switch (detail)
            {
                case IGDBGameDetail.BASE:
                    data = await igdb.GetAsync<BasicGameIgdbDto>(query, IGDBCONSTANTS.URIS.GAMES);
                    var basicDtoList = new List<BasicGameDto>();
                    if (data is not null)
                    {
                        foreach (var game in (List<BasicGameIgdbDto>)data)
                        {
                            basicDtoList.Add(CreateBasicGameDto(game));
                            dataNotEmpty = true;
                        }
                    }
                    data = basicDtoList;
                    break;
                case IGDBGameDetail.DETAIL:
                    data = await igdb.GetAsync<DetailGameIgdbDto>(query, IGDBCONSTANTS.URIS.GAMES);
                    var detailDtoList = new List<DetailGameDto>();
                    if (data is not null)
                    {
                        foreach (var game in (List<DetailGameIgdbDto>)data)
                        {
                            detailDtoList.Add(CreateDetailGameDto(game));
                            dataNotEmpty = true;
                        }
                    }
                    data = detailDtoList;
                    break;
                default:
                    throw new InvalidEnumArgumentException("Detail enum is not valid.");
            }

            var response = dataNotEmpty ? Results.Ok(data) : Results.NotFound();
            
            return response;
        });

        // GET: /api/games - Obtener todos los juegos populares
        app.MapGet("/api/games/trending", async ([AsParameters] IgdbRequest query, BadReviewContext db, IGDBClient igdb) =>
        {
            var responseTrending = await igdb.GetTrendingGamesAsync(query);
            
            if (responseTrending is null || responseTrending.Count == 0)
                return Results.NotFound();


            var gameIds = responseTrending.Select(g => g.Game_id);
            string idsFilter = $"({string.Join(",", gameIds)})";
            
            Console.WriteLine($"Trending game IDs: {idsFilter}");

            var queryGames = new IgdbRequest
            {
                Filters = $"id = {idsFilter}",
                PageSize = query.PageSize
            };
            queryGames.SetDefaults();

            var games = await igdb.GetAsync<BasicGameIgdbDto>(queryGames, IGDBCONSTANTS.URIS.GAMES);
            
            if (games is null || games.Count == 0)
                return Results.NotFound();

            var detailDtoList = games.Select(g => CreateBasicGameDto(g)).ToList();

            return Results.Ok(detailDtoList);
        });

        // GET: /api/games/{id} - Obtener un juego por ID
        app.MapGet("/api/games/{id}", async (int id, BadReviewContext db, IGDBClient igdb) =>
        {
            DetailGameDto? gameDB = await db.Games
                .Where(g => g.Id == id)
                .GameToDetailDto()
                .FirstOrDefaultAsync();

            if (gameDB is not null) Console.WriteLine($"Fetching game: {gameDB.Name}, from DB");
            if (gameDB is not null) return Results.Ok(gameDB);
            else
            {
                var query = new IgdbRequest { Filters = $"id = {id}" };
                query.SetDefaults();

                DetailGameIgdbDto? gameIGDB =
                    (await igdb.GetAsync<DetailGameIgdbDto>(query, IGDBCONSTANTS.URIS.GAMES))?
                        .FirstOrDefault();

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
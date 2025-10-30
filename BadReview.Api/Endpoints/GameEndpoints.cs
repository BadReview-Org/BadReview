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
                .GameToDetailDto()
                .FirstOrDefaultAsync();

            if (gameDB is not null) Console.WriteLine($"Fetching game: {gameDB.Name}, from DB");
            if (gameDB is not null) return Results.Ok(gameDB);
            else
            {
                var query = new SelectGamesRequest { Filters = $"id = {id}", Detail = IGDBFieldsEnum.DETAIL };
                query.SetDefaults();

                DetailGameIgdbDto? gameIGDB = (await igdb.GetGamesAsync<DetailGameIgdbDto>(query))?.FirstOrDefault();

                if (gameIGDB is null) return Results.NotFound(id);
                else
                {
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
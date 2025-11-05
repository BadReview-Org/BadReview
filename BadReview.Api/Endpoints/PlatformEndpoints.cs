#if false
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
            var platformsIgdb = await igdb.GetPlatformsAsync<PlatformIgdbDto>(query);

            List<PlatformDto>? platformsList = platformsIgdb?.Select(p => CreatePlatformDto(p)).ToList();

            var response = platformsList is null || platformsList.Count == 0
                ? Results.Ok(platformsList) : Results.NotFound("No platforms matching the query filters");

            return response;
        });

        // GET: /api/platforms/{id} - Obtener una plataforma por ID
        app.MapGet("/api/platforms/{id}", async (int id, BadReviewContext db, IGDBClient igdb) =>
        {
            PlatformDto? platDB = await db.Platforms
                .Where(p => p.Id == id)
                .Select(p => new PlatformDto(
                    p.Id, p.Name, p.Abbreviation, p.Generation, p.Summary,
                    p.Logo?.ImageId, p.Logo?.ImageHeight, p.Logo?.ImageWidth,
                    p.GamePlatforms.Select(gp => CreateBasicGameDto(gp.Game)).ToList()
                ))
                .FirstOrDefaultAsync();

            if (platDB is not null) Console.WriteLine($"Fetching platform: {platDB.Name}, from DB");
            if (platDB is not null) return Results.Ok(platDB);
            else
            {
                var query = new IgdbRequest { Filters = $"id = {id}" };
                query.SetDefaults();

                PlatformIgdbDto? platIGDB = (await igdb.GetPlatformsAsync<PlatformIgdbDto>(query))?.FirstOrDefault();

                if (platIGDB is null) return Results.NotFound($"No platform matching the ID: {id}");
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
#endif
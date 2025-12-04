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
    public static WebApplication MapPlatformEndpoints(this WebApplication app)
    {
        // GET: /api/platforms - Obtener todas las plataformas
        app.MapGet("/api/platforms", GetPlatforms)
            .WithName("GetPlatforms")
            .WithTags("Platforms")
            .WithSummary("Get all platforms")
            .WithDescription("Retrieve a paginated list of all platforms in the system");
        // GET: /api/platforms/{id} - Obtener una plataforma por ID

        app.MapGet("/api/platforms/{id}", GetPlatformById)
            .WithName("GetPlatformById")
            .WithTags("Platforms")
            .WithSummary("Get platform by ID")
            .WithDescription("Retrieve a specific platform by its ID");

        return app;
    }

    static async Task<IResult> GetPlatforms
    ([AsParameters] PaginationRequest pag, [AsParameters] IgdbRequest query, IPlatformService platformService)
    {
        //query.SetDefaults();
        pag.SetDefaults();

        var platformPage = await platformService.GetPlatformsAsync(query, pag);

        return Results.Ok(platformPage);
    }

    static async Task<IResult> GetPlatformById
    (int id, IPlatformService platformService)
    {
        try
        {
            var platform = await platformService.GetPlatformByIdAsync(id, true);

            var response = platform is null ?
                Results.NotFound($"No platform with id {id}") : Results.Ok(platform);

            return response;
        }
        catch (WritingToDBException ex)
            { return Results.InternalServerError($"Error while persisting data to DB: {ex.Message}"); }
        catch (Exception ex)
            { return Results.InternalServerError($"Unexpected exception: {ex.Message}"); }
    }
}
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

public static class DeveloperEndpoints
{
    public static void MapDeveloperEndpoints(this WebApplication app)
    {
        // GET: /api/genres - Obtener todos los generos
        app.MapGet("/api/developers", GetDevelopers);

        app.MapGet("/api/developer/{id}", GetDeveloperById);
    }

    static async Task<IResult> GetDevelopers
    ([AsParameters] PaginationRequest pag, [AsParameters] IgdbRequest query, IDeveloperService developerService)
    {
        //query.SetDefaults();
        pag.SetDefaults();

        var devPage = await developerService.GetDevelopersAsync(query, pag);

        return Results.Ok(devPage);
    }

    static async Task<IResult> GetDeveloperById
    (int id, IDeveloperService developerService)
    {
        var developer = await developerService.GetDeveloperByIdAsync(id, true);

        var response = developer is null ?
            Results.NotFound($"No developer with id {id}") : Results.Ok(developer);

        return response;
    }
}
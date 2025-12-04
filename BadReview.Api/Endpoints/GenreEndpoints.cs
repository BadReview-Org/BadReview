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

public static class GenreEndpoints
{
    public static WebApplication MapGenreEndpoints(this WebApplication app)
    {
        // GET: /api/genres - Obtener todos los generos
        app.MapGet("/api/genres", GetGenres)
            .WithName("GetGenres")
            .WithTags("Genres")
            .WithSummary("Get all genres")
            .WithDescription("Retrieve a paginated list of all genres in the system");

        app.MapGet("/api/genres/{id}", GetGenreById)
            .WithName("GetGenreById")
            .WithTags("Genres")
            .WithSummary("Get genre by ID")
            .WithDescription("Retrieve a specific genre by its ID");

        return app;
    }

    static async Task<IResult> GetGenres
    ([AsParameters] PaginationRequest pag, [AsParameters] IgdbRequest query, IGenreService genreService)
    {
        //query.SetDefaults();
        pag.SetDefaults();

        var genrePage = await genreService.GetGenresAsync(query, pag);

        return Results.Ok(genrePage);
    }

    static async Task<IResult> GetGenreById
    (int id, IGenreService genreService)
    {
        if (id < 0) return Results.BadRequest($"Genre id can't be negative, received id: {id}");

        try
        {
            var genre = await genreService.GetGenreByIdAsync(id, true);

            var response = genre is null ?
                Results.NotFound($"No genre with id {id}") : Results.Ok(genre);

            return response;
        }
        catch (WritingToDBException ex)
            { return Results.InternalServerError($"Error while persisting data to DB: {ex.Message}"); }
        catch (Exception ex)
            { return Results.InternalServerError($"Unexpected exception: {ex.Message}"); }
    }
}
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
    public static void MapGenreEndpoints(this WebApplication app)
    {
        // GET: /api/genres - Obtener todos los generos
        app.MapGet("/api/genres", GetGenres);

        app.MapGet("/api/genres/{id}", GetGenreById);
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
        catch (WritingToDBException ex) { return Results.InternalServerError(ex.Message); }
        catch (Exception ex) { return Results.InternalServerError($"Unexpected exception: {ex.Message}"); }
    }
}
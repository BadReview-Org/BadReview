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
        app.MapGet("/api/genres", async ([AsParameters] IgdbRequest query, BadReviewContext db, IGDBClient igdb) =>
        {
            var genresIgdb = await igdb.GetGenresAsync(query);

            List<GenreDto>? genreList = genresIgdb?.Select(gen => CreateGenreDto(gen)).ToList();

            var response = Results.Ok(genreList);

            return response;
        });
    }
}
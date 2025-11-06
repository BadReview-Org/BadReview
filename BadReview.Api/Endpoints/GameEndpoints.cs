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
        app.MapGet("/api/games", GetGames);

        // GET: /api/games - Obtener todos los juegos populares
        app.MapGet("/api/games/trending", GetTrendingGames);

        // GET: /api/games/{id} - Obtener un juego por ID
        app.MapGet("/api/games/{id}", GetGameById);
    }

    // Handlers

    private static async Task<IResult> GetGames
    ([AsParameters] PaginationRequest pag, [AsParameters] IgdbRequest query, IGameService gameService)
    {
        query.SetDefaults();
        pag.SetDefaults();

        List<BasicGameDto> games = await gameService.GetGamesAsync(query, pag);

        var response = games.Count > 0 ?
            Results.Ok(games) : Results.NotFound("No games matching the query filters.");

        return response;
    }

    private static async Task<IResult> GetTrendingGames
    ([AsParameters] PaginationRequest pag, [AsParameters] IgdbRequest query, IGameService gameService)
    {
        query.SetDefaults();
        pag.SetDefaults();

        List<BasicGameDto> trendingGames = await gameService.GetTrendingGamesAsync(query, pag);

        var response = trendingGames.Count > 0 ?
            Results.Ok(trendingGames) : Results.NotFound("Unable to find trending games on IGDB");

        return response;
    }
    
    private static async Task<IResult> GetGameById(int id, IGameService gameService)
    {
        if (id < 0) return Results.BadRequest($"Game id can't be negative, received id: {id}");

        DetailGameDto? game = await gameService.GetGameByIdAsync(id, true);

        var response = game is not null ? Results.Ok(game) : Results.NotFound($"Can't find game with id {id}");

        return response;
    }
}
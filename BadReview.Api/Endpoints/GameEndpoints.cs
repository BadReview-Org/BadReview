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
        //query.SetDefaults();
        pag.SetDefaults();

        var gamesPage = await gameService.GetGamesAsync(query, pag);

        return Results.Ok(gamesPage);
    }

    private static async Task<IResult> GetTrendingGames
    ([AsParameters] PaginationRequest pag, [AsParameters] IgdbRequest query, IGameService gameService)
    {
        //query.SetDefaults();
        pag.SetDefaults();

        var trendingGamesPage = await gameService.GetTrendingGamesAsync(query, pag);

        return Results.Ok(trendingGamesPage);
    }
    
    private static async Task<IResult> GetGameById(int id, IGameService gameService)
    {
        if (id < 0) return Results.BadRequest($"Game id can't be negative, received id: {id}");

        DetailGameDto? game = await gameService.GetGameByIdAsync(id, true);

        var response = game is not null ? Results.Ok(game) : Results.NotFound($"Can't find game with id {id}");

        return response;
    }
}
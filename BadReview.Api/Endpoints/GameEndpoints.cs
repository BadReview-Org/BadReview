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
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace BadReview.Api.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        // GET: /api/games - Obtener todos los juegos
        app.MapGet("/api/games", GetGames);

        // GET: /api/games - Obtener todos los juegos populares
        app.MapGet("/api/games/trending", GetTrendingGames);

        // GET: /api/games/public/{id} - Obtener un juego por ID
        app.MapGet("/api/games/public/{id}", GetGameById);

        // GET: /api/games/private/{id} - Añadir la review del usuario a la información del juego por ID
        app.MapGet("/api/games/private/{id}", GetGameAndUserDataById).RequireAuthorization("AccessTokenPolicy");
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

    private static async Task<IResult> GetGameById
    ([AsParameters] PaginationRequest reviewsPag, int id, bool? onlyReviewPage,
    IGameService gameService, IReviewService reviewService)
    {
        if (id < 0) return Results.BadRequest($"Game id can't be negative, received id: {id}");


        onlyReviewPage ??= false;

        if ((bool)onlyReviewPage)
        {
            var reviewsPage = await reviewService.GetBasicReviewsAsync(reviewsPag, false, GetReviewsOpt.REVIEWS, null, id);

            return Results.Ok(reviewsPage);
        }
        else
        {
            DetailGameDto? game = await gameService.GetGameByIdAsync(id, reviewsPag, true);

            var response = game is not null ? Results.Ok(game) : Results.NotFound($"Can't find game with id {id}");

            return response;
        }
    }

    private static async Task<IResult> GetGameAndUserDataById
    ([AsParameters] PaginationRequest reviewsPag, int id, bool? onlyReviewPage, ClaimsPrincipal userClaims,
    IGameService gameService, IReviewService reviewService)
    {
        if (id < 0) return Results.BadRequest($"Game id can't be negative, received id: {id}");

        string? claimUserId =
            userClaims.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (claimUserId is null) return Results.BadRequest("Can't retrieve user id from JWT claims.");


        int userId = int.Parse(claimUserId);
        
        onlyReviewPage ??= false;

        if ((bool)onlyReviewPage)
        {
            var reviewsPage = await reviewService.GetBasicReviewsAsync(reviewsPag, false, GetReviewsOpt.REVIEWS, null, id);

            return Results.Ok(reviewsPage);
        }
        else
        {
            DetailGameDto? game = await gameService.GetGameByIdAsync(id, reviewsPag, true);

            if (game is null) return Results.NotFound($"Can't find game with id {id}");


            var userData =
                await reviewService.GetDetailReviewsAsync(new PaginationRequest(), false, GetReviewsOpt.ALL, userId, id);

            DetailReviewDto? review = userData.Data.FirstOrDefault();

            return Results.Ok(new PrivateDetailGameDto(game, review));
        }
    }
}
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using BadReview.Api.Data;

using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.Utils;
using BadReview.Shared.DTOs.Response;

using static BadReview.Api.Mapper.Mapper;
using System.ComponentModel;

namespace BadReview.Api.Services;

public class GameService : IGameService
{
    private readonly IIGDBService _igdb;
    private readonly BadReviewContext _db;

    public GameService(IIGDBService igdb, BadReviewContext db)
    {
        _igdb = igdb;
        _db = db;
    }

    private async Task<int> GetGameIdReviewCount(int gameId, GetReviewsOpt opt)
    {
        return opt switch
        {
            GetReviewsOpt.REVIEWS => await _db.Reviews.CountAsync(r => r.GameId == gameId && r.IsReview),
            GetReviewsOpt.FAVORITES => await _db.Reviews.CountAsync(r => r.GameId == gameId && r.IsFavorite),
            GetReviewsOpt.ALL => await _db.Reviews.CountAsync(r => r.GameId == gameId),
            _ => throw new InvalidEnumArgumentException()
        };
    }

    private async Task<DetailGameDto?> GetGameByIdDB(int id, PaginationRequest reviewsPag)
    {
        int reviewCount = await GetGameIdReviewCount(id, GetReviewsOpt.REVIEWS);
        int favoritesCount = await GetGameIdReviewCount(id, GetReviewsOpt.FAVORITES);
        return await _db.Games
            .Where(g => g.Id == id)
            .GameToDetailDto(reviewCount, favoritesCount, reviewsPag)
            .FirstOrDefaultAsync();
    }

    private async Task AddRelatedGenres(HashSet<GenreIgdbDto>? genres)
    {
        if (genres is null) return;

        var genreIds = genres.Select(g => g.Id).ToHashSet();

        var existingIds = await _db.Genres
            .Where(g => genreIds.Contains(g.Id))
            .Select(g => g.Id)
            .ToHashSetAsync();

        var newGenres = genres
            .Where(g => !existingIds.Contains(g.Id))
            .Select(g => CreateGenreEntity(g))
            .ToList();

        if (newGenres.Count > 0) _db.Genres.AddRange(newGenres);
    }

    private async Task AddRelatedPlatforms(HashSet<PlatformIgdbDto>? plats)
    {
        if (plats is null) return;

        var platIds = plats.Select(p => p.Id).ToHashSet();

        var existingIds = await _db.Platforms
            .Where(p => platIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToHashSetAsync();

        var newPlats = plats
            .Where(p => !existingIds.Contains(p.Id))
            .Select(p => CreatePlatformEntity(p))
            .ToList();

        if (newPlats.Count > 0) _db.Platforms.AddRange(newPlats);
    }

    private async Task AddRelatedDevelopers(HashSet<InvCompIgdbDto>? companies)
    {
        if (companies is null) return;

        var devs = companies.Where(c => c.Developer).Select(c => c.Company).ToList();
        var devIds = devs.Select(d => d.Id).ToHashSet();

        var existingIds = await _db.Developers
            .Where(d => devIds.Contains(d.Id))
            .Select(d => d.Id)
            .ToHashSetAsync();

        var newDevs = devs
            .Where(d => !existingIds.Contains(d.Id))
            .Select(d => CreateDeveloperEntity(d))
            .ToList();

        if (newDevs.Count > 0) _db.Developers.AddRange(newDevs);
    }


    public async Task<PagedResult<BasicGameDto>> GetGamesAsync(IgdbRequest query, PaginationRequest pag)
    {
        var igdbGames = await _igdb.GetAsync<BasicGameIgdbDto>(query, pag, IGDBCONSTANTS.URIS.GAMES);

        var basicGames = igdbGames.Data.Select(g => CreateBasicGameDto(g)).ToList();

        var gamesPage = new PagedResult<BasicGameDto>(basicGames, igdbGames.TotalCount, igdbGames.Page, igdbGames.PageSize);

        return gamesPage;
    }

    public async Task<PagedResult<BasicGameDto>> GetTrendingGamesAsync(IgdbRequest query, PaginationRequest pag)
    {
        var responseTrending = await _igdb.GetTrendingGamesAsync(query, pag);

        if (responseTrending.Data.Count == 0)
            return new PagedResult<BasicGameDto>([], responseTrending.TotalCount, responseTrending.Page, responseTrending.PageSize);

        var gameIds = responseTrending.Data.Select(g => g.Game_id);
        string idsFilter = $"({string.Join(",", gameIds)})";

        var queryGames = new IgdbRequest { Filters = $"id = {idsFilter}" };
        //queryGames.SetDefaults();

        var pagGames = new PaginationRequest(null, pag.PageSize);

        var igdbGames = await _igdb.GetAsync<BasicGameIgdbDto>(queryGames, pagGames, IGDBCONSTANTS.URIS.GAMES);

        var basicGames = igdbGames.Data.Select(g => CreateBasicGameDto(g)).ToList();

        var gamesPage = new PagedResult<BasicGameDto>(
            basicGames, responseTrending.TotalCount,
            pag.Page ?? CONSTANTS.DEF_PAGE, pag.PageSize ?? CONSTANTS.DEF_PAGESIZE);

        return gamesPage;
    }

    public async Task<DetailGameDto?> GetGameByIdAsync(int id, PaginationRequest reviewsPag, bool cache)
    {
        DetailGameDto? gameDB = await GetGameByIdDB(id, reviewsPag);

        if (gameDB is not null) Console.WriteLine($"Fetching game: {gameDB.Name}, from DB");
        if (gameDB is not null) return gameDB;
        

        var query = new IgdbRequest { Filters = $"id = {id}" };
        //query.SetDefaults();

        PagedResult<DetailGameIgdbDto> response =
            await _igdb.GetAsync<DetailGameIgdbDto>(query, new PaginationRequest(), IGDBCONSTANTS.URIS.GAMES);

        DetailGameIgdbDto? gameIGDB = response.Data.FirstOrDefault();

        if (gameIGDB is null) return null;


        //mapear a Game y persistir (si cache == true)
        if (cache)
        {
            var newGame = CreateGameEntity(gameIGDB);
            
            // Investigar: es EF thread-safe para juntar los 3 primeros awaits?
            await AddRelatedGenres(gameIGDB.Genres);
            await AddRelatedPlatforms(gameIGDB.Platforms);
            await AddRelatedDevelopers(gameIGDB.Involved_Companies);
            _db.Games.Add(newGame);

            if (await _db.SafeSaveChangesAsync())
                throw new WritingToDBException("Exception while saving new game from IGDB to DB.");

            Console.WriteLine($"Cached IGDB game: {gameIGDB.Name} into the database");
        }

        return CreateDetailGameDto(gameIGDB, reviewsPag);
    }
}
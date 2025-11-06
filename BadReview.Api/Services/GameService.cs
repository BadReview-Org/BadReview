using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using BadReview.Api.Data;
using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.Utils;
using BadReview.Shared.DTOs.Response;

using static BadReview.Api.Mapper.Mapper;

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

    private async Task<DetailGameDto?> GetGameByIdDB(int id)
    {
        return await _db.Games
            .Where(g => g.Id == id)
            .GameToDetailDto()
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


    public async Task<List<BasicGameDto>> GetGamesAsync(IgdbRequest query, PaginationRequest pag)
    {
        var igdbGames = await _igdb.GetAsync<BasicGameIgdbDto>(query, pag, IGDBCONSTANTS.URIS.GAMES);
        var basicGames = new List<BasicGameDto>();

        if (igdbGames is not null && igdbGames.Count > 0)
            basicGames = igdbGames.Select(g => CreateBasicGameDto(g)).ToList();

        return basicGames;
    }

    public async Task<List<BasicGameDto>> GetTrendingGamesAsync(IgdbRequest query, PaginationRequest pag)
    {
        var responseTrending = await _igdb.GetTrendingGamesAsync(query, pag);

        if (responseTrending is null || responseTrending.Count == 0)
            return new List<BasicGameDto>();

        var gameIds = responseTrending.Select(g => g.Game_id);
        string idsFilter = $"({string.Join(",", gameIds)})";

        var queryGames = new IgdbRequest { Filters = $"id = {idsFilter}" };
        queryGames.SetDefaults();

        var pagGames = new PaginationRequest(null, pag.PageSize);

        var igdbGames = await _igdb.GetAsync<BasicGameIgdbDto>(queryGames, pagGames, IGDBCONSTANTS.URIS.GAMES);
        var basicGames = new List<BasicGameDto>();

        if (igdbGames is not null && igdbGames.Count > 0)
            basicGames = igdbGames.Select(g => CreateBasicGameDto(g)).ToList();

        return basicGames;
    }

    public async Task<DetailGameDto?> GetGameByIdAsync(int id, bool cache)
    {
        DetailGameDto? gameDB = await GetGameByIdDB(id);

        if (gameDB is not null) Console.WriteLine($"Fetching game: {gameDB.Name}, from DB");
        if (gameDB is not null) return gameDB;
        

        var query = new IgdbRequest { Filters = $"id = {id}" };
        query.SetDefaults();

        DetailGameIgdbDto? gameIGDB =
            (await _igdb.GetAsync<DetailGameIgdbDto>(query, new PaginationRequest(), IGDBCONSTANTS.URIS.GAMES))?
                .FirstOrDefault();

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
            
            await _db.SaveChangesAsync();
            Console.WriteLine($"Cached IGDB game: {gameIGDB.Name} into the database");
        }

        return CreateDetailGameDto(gameIGDB);
    }
}
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using BadReview.Api.Data;
using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.Utils;
using BadReview.Shared.DTOs.Response;

using static BadReview.Api.Mapper.Mapper;
using BadReview.Api.Models;

namespace BadReview.Api.Services;

public class ReviewService : IReviewService
{
    private readonly IIGDBService _igdb;
    private readonly BadReviewContext _db;

    public ReviewService(IIGDBService igdb, BadReviewContext db)
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

    public async Task<PagedResult<DetailReviewDto>> GetReviewsAsync(PaginationRequest pag)
    {
        var page = pag.Page ?? CONSTANTS.DEF_PAGE;
        var pageSize = pag.PageSize ?? CONSTANTS.DEF_PAGESIZE;

        var count = await _db.Reviews.CountAsync();

        var reviews = await _db.Reviews
            .OrderBy(r => r.Id)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Include(r => r.User)
            .ToListAsync();

        if (reviews is null)
            return new PagedResult<DetailReviewDto>(new(), count, page, pageSize);


        List<DetailReviewDto> reviewList = reviews
            .Select(r => new DetailReviewDto(
                r.Id,
                r.Rating,
                r.StartDate,
                r.EndDate,
                r.ReviewText,
                r.StateEnum,
                r.IsFavorite,
                new BasicUserDto(
                    r.User.Id,
                    r.User.Username,
                    r.User.FullName
                ),
                null,
                r.Date.CreatedAt, r.Date.UpdatedAt
                ))
            .ToList();

        return new PagedResult<DetailReviewDto>(reviewList, count, page, pageSize);
    }

    public async Task<DetailReviewDto?> GetReviewByIdAsync(int id)
    {
        var review = await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null) return null;


        var reviewDto = new DetailReviewDto(
            review.Id,
            review.Rating,
            review.StartDate,
            review.EndDate,
            review.ReviewText,
            review.StateEnum,
            review.IsFavorite,
            new BasicUserDto(
                review.User.Id,
                review.User.Username,
                review.User.FullName
            ),
            new BasicGameDto(
                review.Game.Id,
                review.Game.Name,
                review.Game.Cover?.ImageId,
                review.Game.Cover?.ImageHeight,
                review.Game.Cover?.ImageWidth,
                review.Game.RatingIGDB,
                review.Game.Total_RatingBadReview,
                review.Game.Count_RatingBadReview
            ),
            review.Date.CreatedAt, review.Date.UpdatedAt
        );

        return reviewDto;
    }

    public async Task<DetailReviewDto?> UpdateReviewAsync(int reviewId, int userId, CreateReviewRequest updatedReview)
    {
        var review = await _db.Reviews.Include(r => r.User)
            .Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review is null || userId != review.UserId) return null;

        review.Game.Total_RatingBadReview -= review.Rating ?? 0;
        review.Game.Total_RatingBadReview += updatedReview.Rating ?? 0;

        review.Rating = updatedReview.Rating;
        review.StartDate = updatedReview.StartDate;
        review.EndDate = updatedReview.EndDate;
        review.ReviewText = updatedReview.ReviewText;
        review.StateEnum = updatedReview.StateEnum;
        review.IsFavorite = updatedReview.IsFavorite;

        await _db.SaveChangesAsync();

        var reviewDto = new DetailReviewDto
        (
            review.Id,
            review.Rating,
            review.StartDate,
            review.EndDate,
            review.ReviewText,
            review.StateEnum,
            review.IsFavorite,
            new BasicUserDto(
                review.User.Id,
                review.User.Username,
                review.User.FullName
            ),
            new BasicGameDto(
                review.Game.Id,
                review.Game.Name,
                review.Game.Cover?.ImageId,
                review.Game.Cover?.ImageHeight,
                review.Game.Cover?.ImageWidth,
                review.Game.RatingIGDB,
                review.Game.Total_RatingBadReview,
                review.Game.Count_RatingBadReview
            ),
            review.Date.CreatedAt, review.Date.UpdatedAt
        );

        return reviewDto;
    }

    public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
    {
        var review = await _db.Reviews.Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review is null || review.UserId != userId) return false;


        review.Game.Total_RatingBadReview -= review.Rating ?? 0;
        review.Game.Count_RatingBadReview--;
        
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<DetailReviewDto?> CreateReviewAsync(CreateReviewRequest newReview, User user)
    {
        var game = await _db.Games.FindAsync(newReview.GameId);
        if (game is null) return null;

        if (user.Reviews.Select(r => r.GameId).Contains(game.Id)) return null;


        game.Total_RatingBadReview += newReview.Rating ?? 0;
        game.Count_RatingBadReview++;

        var reviewDb = new Review
        {
            Rating = newReview.Rating,
            StartDate = newReview.StartDate,
            EndDate = newReview.EndDate,
            ReviewText = newReview.ReviewText,
            StateEnum = newReview.StateEnum,
            IsFavorite = newReview.IsFavorite,
            UserId = user.Id,
            GameId = game.Id
        };

        _db.Reviews.Add(reviewDb);
        await _db.SaveChangesAsync();

        /*reviewdb = await db.Reviews
            .AsNoTracking()
            .FirstAsync(r => r.Id == reviewdb.Id);*/
        reviewDb.Date = await _db.Reviews
            .Where(r => r.Id == reviewDb.Id)
            .Select(r => r.Date)
            .AsNoTracking()
            .FirstAsync();

        var reviewDto = new DetailReviewDto
        (
            reviewDb.Id,
            reviewDb.Rating,
            reviewDb.StartDate,
            reviewDb.EndDate,
            reviewDb.ReviewText,
            reviewDb.StateEnum,
            reviewDb.IsFavorite,
            new BasicUserDto(
                user.Id,
                user.Username,
                user.FullName
            ),
            new BasicGameDto(
                game.Id,
                game.Name,
                game.Cover?.ImageId,
                game.Cover?.ImageHeight,
                game.Cover?.ImageWidth,
                game.RatingIGDB,
                game.Total_RatingBadReview,
                game.Count_RatingBadReview
            ),
            reviewDb.Date.CreatedAt, reviewDb.Date.UpdatedAt
        );

        return reviewDto;
    }
}
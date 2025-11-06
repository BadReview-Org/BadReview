using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.DTOs.Response;
using BadReview.Shared.Utils;

namespace BadReview.Api.Services;

public interface IGameService
{
    Task<List<BasicGameDto>> GetGamesAsync(IgdbRequest query);
    Task<List<BasicGameDto>> GetTrendingGamesAsync(IgdbRequest query);
    Task<DetailGameDto?> GetGameByIdAsync(int id, bool cache);
}

public interface IDeveloperService
{

}

public interface IGenreService
{

}

public interface IPlatformService
{

}

public interface IReviewService
{

}

public interface IUserService
{

}

public interface IIGDBService
{
    Task<List<PopularIgdbDto>?> GetTrendingGamesAsync(IgdbRequest query);

    Task<List<GenreIgdbDto>?> GetGenresAsync(IgdbRequest query);

    Task<List<T>?> GetPlatformsAsync<T>(IgdbRequest query);

    Task<List<T>?> GetAsync<T>(IgdbRequest query, string uri);
}

public interface IAuthService
{
    bool VerifyPassword(string username, string password, string hashed);

    string HashPassword(string username, string password);

    string GenerateToken(string username, int userId);
}
using System.Security.Claims;
using BadReview.Api.Models;
using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.DTOs.Response;
using BadReview.Shared.Utils;

namespace BadReview.Api.Services;

public interface IGameService
{
    Task<PagedResult<BasicGameDto>> GetGamesAsync(IgdbRequest query, PaginationRequest pag);
    Task<PagedResult<BasicGameDto>> GetTrendingGamesAsync(IgdbRequest query, PaginationRequest pag);
    Task<DetailGameDto?> GetGameByIdAsync(int id, bool cache);
}

public interface IDeveloperService
{
    Task<PagedResult<BasicDeveloperDto>> GetDevelopersAsync(IgdbRequest query, PaginationRequest pag);
    Task<DetailDeveloperDto?> GetDeveloperByIdAsync(int id, bool cache);
}

public interface IGenreService
{
    Task<PagedResult<GenreDto>> GetGenresAsync(IgdbRequest query, PaginationRequest pag);
    Task<GenreDto?> GetGenreByIdAsync(int id, bool cache);
}

public interface IPlatformService
{
    Task<PagedResult<PlatformDto>> GetPlatformsAsync(IgdbRequest query, PaginationRequest pag);
    Task<PlatformDto?> GetPlatformByIdAsync(int id, bool cache);
}


public enum ReviewCode { OK, REVIEWNOTFOUND, GAMENOTFOUND, USERNOTMATCH, USERALREADYHASREVIEW }
public interface IReviewService
{
    Task<PagedResult<DetailReviewDto>> GetReviewsAsync(PaginationRequest pag);
    Task<DetailReviewDto?> GetReviewByIdAsync(int id);
    Task<(ReviewCode, DetailReviewDto?)> UpdateReviewAsync(int reviewId, int userId, CreateReviewRequest updatedReview);
    Task<ReviewCode> DeleteReviewAsync(int reviewId, int userId);
    Task<(ReviewCode, DetailReviewDto?)> CreateReviewAsync(CreateReviewRequest newReview, User user);
}

public enum UserCode { OK, USERNAMENOTFOUND, PASSDONTMATCH }
public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id); // ?
    Task<(UserCode, string?)> LoginUserAsync(LoginUserRequest req);
    Task<UserCode> DeleteUserAsync(ClaimsPrincipal userClaims);
    Task<(UserCode, BasicUserDto)> UpdateUserAsync(ClaimsPrincipal userClaims, CreateUserRequest req);
    Task<(UserCode, BasicUserDto?, string?)> CreateUserAsync(RegisterUserRequest req);
    Task<(UserCode, PrivateUserDto)> GetUserPrivateData();
    Task<PublicUserDto?> GetUserPublicData();
}

public interface IIGDBService
{
    Task<PagedResult<PopularIgdbDto>> GetTrendingGamesAsync(IgdbRequest query, PaginationRequest pag);

    Task<PagedResult<GenreIgdbDto>> GetGenresAsync(IgdbRequest query, PaginationRequest pag);

    Task<PagedResult<PlatformIgdbDto>> GetPlatformsAsync(IgdbRequest query, PaginationRequest pag);

    Task<PagedResult<BasicCompanyIgdbDto>> GetDevelopersAsync(IgdbRequest query, PaginationRequest pag);

    Task<PagedResult<T>> GetAsync<T>(IgdbRequest query, PaginationRequest pag, string uri);
}

public interface IAuthService
{
    bool VerifyPassword(string username, string password, string hashed);

    string HashPassword(string username, string password);

    string GenerateToken(string username, int userId);
}
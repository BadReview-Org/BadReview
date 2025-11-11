using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using BadReview.Api.Models;
using BadReview.Api.Data;
using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.Utils;
using BadReview.Shared.DTOs.Response;

using static BadReview.Api.Mapper.Mapper;
using static BadReview.Api.Services.IUserService;

namespace BadReview.Api.Services;

public class UserService : IUserService
{
    private readonly IIGDBService _igdb;
    private readonly BadReviewContext _db;
    private readonly IAuthService _auth;

    public UserService(IIGDBService igdb, BadReviewContext db, IAuthService auth)
    {
        _igdb = igdb;
        _db = db;
        _auth = auth;
    }

    public async Task<(UserCode, RegisterUserDto?)> CreateUserAsync(CreateUserRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return (UserCode.USERNAMEALREADYEXISTS, null);

        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return (UserCode.EMAILALREADYEXISTS, null);

        if (req.Password is null) return (UserCode.NULLPASSWORD, null);


        var hashedPassword = _auth.HashPassword(req.Username, req.Password);
        var newUser = new User
        {
            Username = req.Username,
            Email = req.Email,
            Password = hashedPassword,
            FullName = req.FullName,
            Birthday = req.Birthday,
            Country = req.Country
        };

        _db.Users.Add(newUser);

        if (!await _db.SafeSaveChangesAsync())
            throw new WritingToDBException("Exception while saving the new user to DB.");

        var userDto = new BasicUserDto(
            newUser.Id,
            newUser.Username
        );

        var accessToken = _auth.GenerateAccessToken(req.Username, newUser.Id);
        var refreshToken = _auth.GenerateRefreshToken(req.Username, newUser.Id);

        return (UserCode.OK, new RegisterUserDto(userDto, new UserTokensDto(accessToken, refreshToken)));
    }

    public async Task<UserCode> DeleteUserAsync(ClaimsPrincipal userClaims)
    {
        string? claimUserId =
            userClaims.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (claimUserId is null) return UserCode.BADUSERCLAIMS;


        var existingUser = await _db.Users
            .Where(u => u.Id == int.Parse(claimUserId))
            .FirstOrDefaultAsync();

        if (existingUser is null) return UserCode.USERNAMENOTFOUND;


        _db.Users.Remove(existingUser);

        if (!await _db.SafeSaveChangesAsync())
            throw new WritingToDBException("Exception while removing the requested user from the DB.");

        return UserCode.OK;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _db.Users
            .Include(u => u.Reviews)
                .ThenInclude(r => r.Game)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<(UserCode, PrivateUserDto?)> GetUserPrivateData(int userId, PaginationRequest pag)
    {
        var user = await _db.Users.AnyAsync(u => u.Id == userId);

        if (!user) return (UserCode.USERNAMENOTFOUND, null);


        var page = pag.Page ?? CONSTANTS.DEF_PAGE;
        var pageSize = pag.PageSize ?? CONSTANTS.DEF_PAGESIZE;

        int reviewCount = await _db.Reviews.CountAsync(r => r.UserId == userId && r.IsReview);
        int favoriteCount = await _db.Reviews.CountAsync(r => r.UserId == userId && r.IsFavorite);

        PrivateUserDto? userDto = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new PrivateUserDto(
                u.Id, u.Username, u.FullName, u.Birthday, u.Country,
                u.Reviews
                    .Where(r => r.IsReview)
                    .OrderByDescending(r => r.Date.UpdatedAt)
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .Select(r => new DetailReviewDto(
                    r.Id, r.Rating, r.StartDate, r.EndDate, r.ReviewText, r.StateEnum, r.IsFavorite, r.IsReview, null,
                    new BasicGameDto(
                        r.Game.Id, r.Game.Name,
                        r.Game.Cover != null ? r.Game.Cover.ImageId : null,
                        r.Game.Cover != null ? r.Game.Cover.ImageHeight : null,
                        r.Game.Cover != null ? r.Game.Cover.ImageWidth : null,
                        r.Game.RatingIGDB, r.Game.Total_RatingBadReview, r.Game.Count_RatingBadReview),
                        r.Date.CreatedAt, r.Date.UpdatedAt
                )).ToPagedResult(reviewCount, page, pageSize),
                u.Reviews
                    .Where(r => r.IsFavorite)
                    .OrderByDescending(r => r.Date.UpdatedAt)
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .Select(r => new DetailReviewDto(
                    r.Id, r.Rating, r.StartDate, r.EndDate, r.ReviewText, r.StateEnum, r.IsFavorite, r.IsReview, null,
                    new BasicGameDto(
                        r.Game.Id, r.Game.Name,
                        r.Game.Cover != null ? r.Game.Cover.ImageId : null,
                        r.Game.Cover != null ? r.Game.Cover.ImageHeight : null,
                        r.Game.Cover != null ? r.Game.Cover.ImageWidth : null,
                        r.Game.RatingIGDB, r.Game.Total_RatingBadReview, r.Game.Count_RatingBadReview),
                        r.Date.CreatedAt, r.Date.UpdatedAt
                )).ToPagedResult(favoriteCount, page, pageSize),
                u.Date.CreatedAt, u.Date.UpdatedAt
                )
            )
            .FirstOrDefaultAsync();

        return (UserCode.OK, userDto);
    }

    public async Task<(UserCode, PublicUserDto?)> GetUserPublicData(int userId, PaginationRequest pag)
    {
        var user = await _db.Users.AnyAsync(u => u.Id == userId);

        if (!user) return (UserCode.USERNAMENOTFOUND, null);


        var page = pag.Page ?? CONSTANTS.DEF_PAGE;
        var pageSize = pag.PageSize ?? CONSTANTS.DEF_PAGESIZE;

        int reviewCount = await _db.Reviews.CountAsync(r => r.UserId == userId && r.IsReview);
        int favoriteCount = await _db.Reviews.CountAsync(r => r.UserId == userId && r.IsFavorite);

        PublicUserDto? userDto = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new PublicUserDto(
                u.Id, u.Username, u.Country,
                u.Reviews
                    .Where(r => r.IsReview)
                    .OrderByDescending(r => r.Date.UpdatedAt)
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .Select(r => new DetailReviewDto(
                    r.Id, r.Rating, r.StartDate, r.EndDate, r.ReviewText, r.StateEnum, r.IsFavorite, r.IsReview, null,
                    new BasicGameDto(
                        r.Game.Id, r.Game.Name,
                        r.Game.Cover != null ? r.Game.Cover.ImageId : null,
                        r.Game.Cover != null ? r.Game.Cover.ImageHeight : null,
                        r.Game.Cover != null ? r.Game.Cover.ImageWidth : null,
                        r.Game.RatingIGDB, r.Game.Total_RatingBadReview, r.Game.Count_RatingBadReview),
                        r.Date.CreatedAt, r.Date.UpdatedAt
                )).ToPagedResult(reviewCount, page, pageSize),
                u.Reviews
                    .Where(r => r.IsFavorite)
                    .OrderByDescending(r => r.Date.UpdatedAt)
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .Select(r => new DetailReviewDto(
                    r.Id, r.Rating, r.StartDate, r.EndDate, r.ReviewText, r.StateEnum, r.IsFavorite, r.IsReview, null,
                    new BasicGameDto(
                        r.Game.Id, r.Game.Name,
                        r.Game.Cover != null ? r.Game.Cover.ImageId : null,
                        r.Game.Cover != null ? r.Game.Cover.ImageHeight : null,
                        r.Game.Cover != null ? r.Game.Cover.ImageWidth : null,
                        r.Game.RatingIGDB, r.Game.Total_RatingBadReview, r.Game.Count_RatingBadReview),
                        r.Date.CreatedAt, r.Date.UpdatedAt
                )).ToPagedResult(reviewCount, page, pageSize),
                u.Date.CreatedAt
                )
            )
            .FirstOrDefaultAsync();

        return (UserCode.OK, userDto);
    }

    public async Task<(UserCode, BasicUserDto?)> UpdateUserAsync(ClaimsPrincipal userClaims, CreateUserRequest req)
    {
        string? claimUserId =
            userClaims.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (claimUserId is null) return (UserCode.BADUSERCLAIMS, null);

        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(claimUserId));

        if (existingUser is null) return (UserCode.USERNAMENOTFOUND, null);

        // Validar que Username sea único (excluyendo el usuario actual)
        if (await _db.Users.AnyAsync(u => u.Username == req.Username && u.Id != existingUser.Id))
            return (UserCode.USERNAMEALREADYEXISTS, null);

        // Validar que Email sea único (excluyendo el usuario actual)
        if (await _db.Users.AnyAsync(u => u.Email == req.Email && u.Id != existingUser.Id))
            return (UserCode.EMAILALREADYEXISTS, null);


        existingUser.Username = req.Username;
        existingUser.Email = req.Email;
        existingUser.FullName = req.FullName;
        existingUser.Birthday = req.Birthday;
        existingUser.Country = req.Country;

        if (req.Password is null)
            existingUser.Password = _auth.HashPassword(existingUser.Username, existingUser.Password);
        else
            existingUser.Password = _auth.HashPassword(existingUser.Username, req.Password);

        if (!await _db.SafeSaveChangesAsync())
            throw new WritingToDBException("Exception while updating the user information in the DB.");

        return (UserCode.OK, new BasicUserDto(existingUser.Id, existingUser.Username));
    }

    public async Task<(UserCode, UserTokensDto?)> LoginUserAsync(LoginUserRequest req)
    {
        var user = await _db.Users
            .Where(u => u.Username == req.Username)
            .Select(u => new { u.Id, u.Username, u.Password })
            .FirstOrDefaultAsync();

        if (user is null) return (UserCode.USERNAMENOTFOUND, null);

        var validHash = _auth.VerifyPassword(req.Username, req.Password, user.Password);

        if (!validHash) return (UserCode.PASSDONTMATCH, null);

        string accessToken = _auth.GenerateAccessToken(req.Username, user.Id);
        string refreshToken = _auth.GenerateRefreshToken(req.Username, user.Id);

        return (UserCode.OK, new UserTokensDto(accessToken, refreshToken));
    }

    public async Task<(UserCode, UserTokensDto?)> RefreshTokens(ClaimsPrincipal refreshTokenClaims)
    {
        string? claimUserId =
            refreshTokenClaims.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

        if (claimUserId is null) return (UserCode.BADUSERCLAIMS, null);

        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(claimUserId));

        if (existingUser is null) return (UserCode.USERNAMENOTFOUND, null);


        string newAccessToken = _auth.GenerateAccessToken(existingUser.Username, existingUser.Id);
        string newRefreshToken = _auth.GenerateRefreshToken(existingUser.Username, existingUser.Id);
        
        return (UserCode.OK, new UserTokensDto(newAccessToken, newRefreshToken));
    }
}
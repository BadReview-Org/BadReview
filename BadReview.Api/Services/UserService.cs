using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using BadReview.Api.Data;
using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.Utils;
using BadReview.Shared.DTOs.Response;

using static BadReview.Api.Mapper.Mapper;
using BadReview.Api.Models;
using System.Security.Claims;

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

    public Task<(UserCode, BasicUserDto?, string?)> CreateUserAsync(RegisterUserRequest req)
    {
        throw new NotImplementedException();
    }

    public Task<UserCode> DeleteUserAsync(ClaimsPrincipal userClaims)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _db.Users
            .Include(u => u.Reviews)
                .ThenInclude(r => r.Game)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task<(UserCode, PrivateUserDto)> GetUserPrivateData()
    {
        throw new NotImplementedException();
    }

    public Task<PublicUserDto?> GetUserPublicData()
    {
        throw new NotImplementedException();
    }

    public Task<(UserCode, string?)> LoginUserAsync(LoginUserRequest req)
    {
        throw new NotImplementedException();
    }

    public Task<(UserCode, BasicUserDto)> UpdateUserAsync(ClaimsPrincipal userClaims, CreateUserRequest req)
    {
        throw new NotImplementedException();
    }

    public async Task<(UserCode, string?)> UserLogin(LoginUserRequest req)
    {
        var user = await _db.Users
            .Where(u => u.Username == req.Username)
            .Select(u => new { u.Id, u.Username, u.Password })
            .FirstOrDefaultAsync();

        if (user is null) return (UserCode.USERNAMENOTFOUND, null);

        var validHash = _auth.VerifyPassword(req.Username, req.Password, user.Password);

        if (!validHash) return (UserCode.PASSDONTMATCH, null);

        string token = _auth.GenerateToken(req.Username, user.Id);

        return (UserCode.OK, token);
    }
}
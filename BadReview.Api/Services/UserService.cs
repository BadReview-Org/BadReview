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

public class UserService : IUserService
{
    private readonly IIGDBService _igdb;
    private readonly BadReviewContext _db;

    public UserService(IIGDBService igdb, BadReviewContext db)
    {
        _igdb = igdb;
        _db = db;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _db.Users
            .Include(u => u.Reviews)
                .ThenInclude(r => r.Game)
            .FirstOrDefaultAsync(u => u.Id == id);
    } 
}
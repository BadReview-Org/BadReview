using Microsoft.EntityFrameworkCore;

using BadReview.Api.Data;
using BadReview.Shared.DTOs.Request;

namespace BadReview.Api.Services.Validations;

public class CheckAvailableCredentials : ValidatorRules.ICheckAvailables
{
    private readonly BadReviewContext _db;

    public CheckAvailableCredentials(BadReviewContext db) { _db = db; }
    
    public async Task<bool> UsernameAvailable(string? username) => !await _db.Users.AnyAsync(u => u.Username == username);

    public async Task<bool> EmailAvailable(string? email) => !await _db.Users.AnyAsync(u => u.Email == email);
}
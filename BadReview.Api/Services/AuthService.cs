using BadReview.Shared.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BadReview.Api.Services;

public class AuthService : IAuthService
{
    public class Dummy;
    private readonly Dummy _dummy = new();

    private readonly PasswordHasher<Dummy> _hasher = new();
    private readonly string _key;
    private readonly string _issuer;

    public AuthService(IConfiguration config)
    {
        _key = config["Jwt:Key"] ?? throw new Exception("Private key not set.");
        _issuer = config["Jwt:Issuer"] ?? throw new Exception("Issuer not set.");
    }

    public bool VerifyPassword(string password, string hashed)
    {
        var result = _hasher.VerifyHashedPassword(_dummy, hashed, password);
        return result == PasswordVerificationResult.Success;
    }

    public string HashPassword(string password)
        => _hasher.HashPassword(_dummy, password);

    private string GenerateToken(string username, int userId, string type, double hours)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", userId.ToString()),
            new Claim("token_type", type)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(hours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateAccessToken(string username, int userId) =>
        GenerateToken(username, userId, CONSTANTS.ACCESSTOKEN, 1);

    public string GenerateRefreshToken(string username, int userId) =>
        GenerateToken(username, userId, CONSTANTS.REFRESHTOKEN, 24 * 30);
}
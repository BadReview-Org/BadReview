using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BadReview.Api.Services;

public class AuthService
{
    private readonly PasswordHasher<string> _hasher = new();
    private readonly string _key;
    private readonly string _issuer;

    public AuthService(IConfiguration config)
    {
        _key = config["Jwt:Key"] ?? throw new Exception("Private key not set.");
        _issuer = config["Jwt:Issuer"] ?? throw new Exception("Issuer not set.");
    }

    // Verifica la contraseña (usarías esto con una BD real)
    public bool VerifyPassword(string username, string password, string hashed)
    {
        var result = _hasher.VerifyHashedPassword(username, hashed, password);
        return result == PasswordVerificationResult.Success;
    }

    // Hashea una contraseña nueva (para registrar usuarios)
    public string HashPassword(string username, string password)
        => _hasher.HashPassword(username, password);

    public string GenerateToken(string username, int userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
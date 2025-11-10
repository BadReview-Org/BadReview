using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class JWTAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthService authService;

    public JWTAuthStateProvider(AuthService authService)
    {
        this.authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {

        var token = await authService.GetTokenAsync(AuthService.AccessKey);
        if (string.IsNullOrEmpty(token))
        {
            await authService.RefreshTokenAsync();
            token = await authService.GetTokenAsync(AuthService.AccessKey);
        }
            

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt;

        try
        {
            jwt = handler.ReadJwtToken(token);
        }
        catch
        {
            Console.WriteLine("Token is malformed.");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(jwt.Claims, "JWT");
        var user = new ClaimsPrincipal(identity);
        Console.WriteLine("Token is correctly set. Retrieving information.");
        return new AuthenticationState(user);
    }

    public void NotifyAuthStateChanged()
    {
        Console.WriteLine(">>> NotifyUserChanged() called!");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

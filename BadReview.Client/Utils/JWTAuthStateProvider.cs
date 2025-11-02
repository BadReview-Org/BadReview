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
        var token = await authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt;

        try
        {
            jwt = handler.ReadJwtToken(token);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(jwt.Claims, "JWT");
        var user = new ClaimsPrincipal(identity);
        
        return new AuthenticationState(user);
    }

    public void NotifyAuthStateChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}

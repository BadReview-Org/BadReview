using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.DTOs.Response;
using Microsoft.AspNetCore.Components.Authorization;

public class AuthService
{
    private readonly IJSRuntime js;
    private readonly HttpClient http;
    private const string TokenKey = "JWT";

    public AuthService(IJSRuntime js, HttpClient http)
    {
        this.js = js;
        this.http = http;
    }

    public async Task<bool> LoginAsync(JWTAuthStateProvider prov, LoginUserRequest request)
    {
        var response = await http.PostAsJsonAsync("api/login", request);
        if (!response.IsSuccessStatusCode)
            return false;

        var content = await response.Content.ReadFromJsonAsync<LoginUserDto>();
        if (content?.Token is null || content.Token is null)
            return false;

        Console.WriteLine($"Received token:\n{content.Token}");

        await js.InvokeVoidAsync("localStorage.setItem", TokenKey, content.Token);

        prov.NotifyAuthStateChanged();
        return true;
    }

    public async Task LogoutAsync(JWTAuthStateProvider prov)
    {
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        prov.NotifyAuthStateChanged();
    }

    public async Task<string?> GetTokenAsync() => await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);

    public void LogJWToken(AuthenticationState state, ILogger logger)
    {
        logger.LogInformation("User's JWT data:");
        logger.LogInformation($"User: {state.User}");
        logger.LogInformation($"Identity: {state.User.Identity}");
        logger.LogInformation($"Is authenticated: {state.User.Identity?.IsAuthenticated}");
        logger.LogInformation($"Authentication type: {state.User.Identity?.AuthenticationType}");

        logger.LogInformation("-------------- Claims --------------");
        foreach (Claim claim in state.User.Claims) logger.LogInformation($"Type: {claim.Type}, Value: {claim.Value}");
        logger.LogInformation("------------------------------------");
    }
}
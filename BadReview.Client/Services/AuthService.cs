using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

using BadReview.Shared.DTOs.Request;
using BadReview.Shared.DTOs.Response;

namespace BadReview.Client.Services;

public class AuthService
{
    private readonly IJSRuntime js;
    private readonly HttpClient http;
    public const string AccessKey = "accessToken";

    public const string RefreshKey = "refreshToken";


    public AuthService(IJSRuntime js, HttpClient http)
    {
        this.js = js;
        this.http = http;
    }

    public async Task<bool> RegisterAsync(JWTAuthStateProvider prov, CreateUserRequest request)
    {
        var response = await http.PostAsJsonAsync("api/register", request);
        if (!response.IsSuccessStatusCode)
            return false;

        var content = await response.Content.ReadFromJsonAsync<RegisterUserDto>();
        if (content is null)
            return false;

        Console.WriteLine($"User registered successfully:\n{content.UserDto}\nReceived" +
        $" token:\n Access token: {content.LoginDto.AccessToken}\n Refresh token: {content.LoginDto.RefreshToken}");
        
        await js.InvokeVoidAsync("localStorage.setItem", AccessKey, content.LoginDto.AccessToken);
        await js.InvokeVoidAsync("localStorage.setItem", RefreshKey, content.LoginDto.RefreshToken);

        prov.NotifyAuthStateChanged();
        return true;
    }

    public async Task<bool> LoginAsync(JWTAuthStateProvider prov, LoginUserRequest request)
    {
        var response = await http.PostAsJsonAsync("api/login", request);
        if (!response.IsSuccessStatusCode)
            return false;

        var content = await response.Content.ReadFromJsonAsync<UserTokensDto>();
        if (content is null)
            return false;

        Console.WriteLine($"Received an access token:\n{content.AccessToken}");
        Console.WriteLine($"Received a refresh token:\n{content.RefreshToken}");
        await js.InvokeVoidAsync("localStorage.setItem", AccessKey, content.AccessToken);
        await js.InvokeVoidAsync("localStorage.setItem", RefreshKey, content.RefreshToken);

        prov.NotifyAuthStateChanged();
        return true;
    }

    public async Task LogoutAsync(JWTAuthStateProvider prov)
    {
        await js.InvokeVoidAsync("localStorage.removeItem", AccessKey);
        await js.InvokeVoidAsync("localStorage.removeItem", RefreshKey);
        prov.NotifyAuthStateChanged();
    }

    public async Task<string?> GetTokenAsync(string key) => await js.InvokeAsync<string?>("localStorage.getItem", key);

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

    public async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await GetTokenAsync(RefreshKey);
        if (string.IsNullOrEmpty(refreshToken))
            return false;
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/refresh");
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshToken);

        var response = await http.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
            return false;
        
        var tokens = await response.Content.ReadFromJsonAsync<UserTokensDto>();
        await js.InvokeVoidAsync("localStorage.setItem", AccessKey, tokens?.AccessToken);
        await js.InvokeVoidAsync("localStorage.setItem", RefreshKey, tokens?.RefreshToken);

        return true;
    }
    
}
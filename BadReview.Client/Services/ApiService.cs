using BadReview.Shared.DTOs.Response;
using BadReview.Client.Utils;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;
using BadReview.Shared.DTOs.Request;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Threading.Tasks;

namespace BadReview.Client.Services;
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public ApiService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<PagedResult<T>> GetManyAsync<T>(ApiRequest request)
    {
        request.SetDefaults();
        string orderby = Uri.EscapeDataString(request.OrderBy ?? "");
        string filters = Uri.EscapeDataString(request.Filters ?? "");
        Console.WriteLine($"Filters after escape: {filters}");
        string queryString = $"?filters={filters}&orderby={orderby}&order={request.Order}&page={request.Page}&pageSize={request.PageSize}";
        var response = await _httpClient.GetFromJsonAsync<PagedResult<T>>($"api/{request.URI}{queryString}");
        return response ?? new PagedResult<T>(new List<T>(), 0, 0, 0);
    }

    public async Task<T?> GetByIdAsync<T>(string uri, int id)
    {
        var response = await _httpClient.GetFromJsonAsync<T>($"api/{uri}/{id}");
        return response;
    }
    public async Task<T?> PostAsync<T>(string uri, object data)
    {
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "JWT");
        // Crear el request con el header de autorización
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/{uri}");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        var response = await _httpClient.PostAsJsonAsync($"api/{uri}", data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        return default;
    }


}
using BadReview.Shared.DTOs.Response;
using BadReview.Client.Utils;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;
using BadReview.Shared.DTOs.Request;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BadReview.Client.Services;
public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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


}
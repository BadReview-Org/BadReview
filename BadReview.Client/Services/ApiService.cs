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
using System.Net.Http.Headers;
using System.Net;
using System.Linq.Expressions;

namespace BadReview.Client.Services;
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    private readonly AuthService _authService;

    private bool _isRefreshing = false;

    public ApiService(HttpClient httpClient, IJSRuntime jsRuntime, AuthService authService)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
        _authService = authService;
    }

    public async Task<PagedResult<T>> GetManyAsync<T>(ApiRequest request)
    {
        request.SetDefaults();
        string orderby = Uri.EscapeDataString(request.OrderBy ?? "");
        string filters = Uri.EscapeDataString(request.Filters ?? "");

        string queryString = $"?filters={filters}&orderby={orderby}&order={request.Order}&page={request.Page}&pageSize={request.PageSize}";
        var response = await _httpClient.GetFromJsonAsync<PagedResult<T>>($"api/{request.URI}{queryString}");
        return response ?? new PagedResult<T>(new List<T>(), 0, 0, 0);
    }


    public async Task<T?> PublicGetByIdAsync<T>(string uri, int id)
    {
        return await _httpClient.GetFromJsonAsync<T>($"{uri}/{id}");;
    }

    public async Task<T?> PrivateGetByIdAsync<T>(string uri, int id)
    {
        return await RefreshTokenRequestAsync<T,T>($"{uri}/{id}", default, HttpMethod.Get);
    }

    public async Task<T?> PrivateGetAsync<T>(string uri)
    {
        return await RefreshTokenRequestAsync<T,T>(uri, default, HttpMethod.Get);
    }

    public async Task<U?> PostAsync<T,U>(string uri, T data)
    {
        return await RefreshTokenRequestAsync<T,U>(uri, data, HttpMethod.Post);
    }

    public async Task<U?> PutAsync<T,U>(string uri, T data)
    {
        return await RefreshTokenRequestAsync<T,U>(uri, data, HttpMethod.Put);
    }

    public async Task<bool> DeleteAsync(string uri)
    {
        return await RefreshTokenRequestAsync(uri, HttpMethod.Delete);
    }

    public async Task<bool> RefreshTokenRequestAsync(string uri, HttpMethod method)
    {
        // 1. Agregar access token
        var token = await _authService.GetTokenAsync(AuthService.AccessKey);
        HttpRequestMessage request = new HttpRequestMessage(method, $"api/{uri}");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // 2. Enviar request
        var response = await _httpClient.SendAsync(request);

        // 3. Si es 401 y no estamos ya refrescando
        if (response.StatusCode == HttpStatusCode.Unauthorized && !_isRefreshing)
        {
            _isRefreshing = true;

            try
            {
                // 4. Intentar refrescar el token
                var refreshed = await _authService.RefreshTokenAsync();

                if (refreshed)
                {
                    // 5. Reintentar el request original con nuevo token
                    var newToken = await _authService.GetTokenAsync(AuthService.AccessKey);
                    request.Headers.Remove("Authorization");
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", newToken);

                    response = await _httpClient.SendAsync(request);
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<U?> RefreshTokenRequestAsync<T,U>(string uri, T? data, HttpMethod method = null!)
    {
        // 1. Agregar access token
        var token = await _authService.GetTokenAsync(AuthService.AccessKey);
        HttpRequestMessage request = new HttpRequestMessage(method ?? HttpMethod.Post, $"api/{uri}");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        if (data != null)
            request.Content = JsonContent.Create(data);

        // 2. Enviar request
        var response = await _httpClient.SendAsync(request);
        // Capturamos la excepcion 401 

        // 3. Si es 401 y no estamos ya refrescando

        if (response.StatusCode == HttpStatusCode.Unauthorized && !_isRefreshing)
        {
            _isRefreshing = true;

            try
            {
                // 4. Intentar refrescar el token
                var refreshed = await _authService.RefreshTokenAsync();

                if (refreshed)
                {
                    // 5. Reintentar el request original con nuevo token


                    var newToken = await _authService.GetTokenAsync(AuthService.AccessKey);
                    request.Headers.Remove("Authorization");
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", newToken);

                    response = await _httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error after refreshing token: {response.StatusCode}");
                    }
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }
        var serializeResponse = await response.Content.ReadFromJsonAsync<U>();
        return serializeResponse;
    }


}


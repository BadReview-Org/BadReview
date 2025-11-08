using System.Net;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.Utils;
using BadReview.Shared.DTOs.Response;

namespace BadReview.Api.Services;

public class IGDBClient : IIGDBService
{
    private static readonly SemaphoreSlim _tokenSemaphore = new(1, 1);
    private readonly HttpClient _httpClient;

    private string? _clientId;
    private string? _clientSecret;

    private static string? _accessTokenURI = null;
    private static string? _accessToken = null;

    public IGDBClient(HttpClient client, IConfiguration config)
    {
        _httpClient = client;
        _clientId = config["IGDB:ClientId"];
        _clientSecret = config["IGDB:ClientSecret"];

        _accessTokenURI = config["IGDB:TokenURI"];

        if (string.IsNullOrWhiteSpace(_clientId) || string.IsNullOrWhiteSpace(_clientSecret)
            || string.IsNullOrWhiteSpace(_accessTokenURI))
            throw new Exception("Can't retrieve IGDB credentials");
    }

    private async Task SetAccessToken()
    {
        // because _accessToken is shared between instances, we use a semaphore to access it safely
        await _tokenSemaphore.WaitAsync();

        // we check again if it's null, in case another instance already updated it
        if (_accessToken is null)
        {
            // we try to get a new access token
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string query = $"{_accessTokenURI}?client_id={_clientId}&client_secret={_clientSecret}&grant_type=client_credentials";

            HttpResponseMessage response = await _httpClient.PostAsync(query, null);

            _httpClient.DefaultRequestHeaders.Accept.Remove(new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var token = await response.Content.ReadFromJsonAsync<GetCredentialsDto>(jsonOptions)
                ?? throw new Exception("Didn't receive a valid IGDB access token");

            Console.WriteLine($"Get new access token for IGDB:\n{token}");

            _accessToken = token.access_token;
        }

        // release the semaphore
        _tokenSemaphore.Release();
    }


    public async Task<PagedResult<BasicCompanyIgdbDto>> GetDevelopersAsync(IgdbRequest query, PaginationRequest pag)
    {
        IgdbRequest queryDevs = new IgdbRequest
        {
            Filters = $"{query.Filters} & developed != null",
            OrderBy = query.OrderBy ?? "name",
            Order = query.Order ?? SortOrder.ASC
        };
        //queryDevs.SetDefaults();

        return await GetAsync<BasicCompanyIgdbDto>(queryDevs, pag, IGDBCONSTANTS.URIS.DEVELOPERS);
    }

    public async Task<PagedResult<PlatformIgdbDto>> GetPlatformsAsync(IgdbRequest query, PaginationRequest pag)
    {
        IgdbRequest queryDevs = new IgdbRequest
        {
            Filters = query.Filters,
            OrderBy = query.OrderBy ?? "name",
            Order = query.Order ?? SortOrder.ASC
        };
        //queryDevs.SetDefaults();

        return await GetAsync<PlatformIgdbDto>(queryDevs, pag, IGDBCONSTANTS.URIS.PLATFORMS);
    }

    public async Task<PagedResult<PopularIgdbDto>> GetTrendingGamesAsync(IgdbRequest query, PaginationRequest pag)
    {
        IgdbRequest queryTrending = new IgdbRequest
        {
            Filters = "popularity_type = 3",
            OrderBy = "value"
        };
        //queryTrending.SetDefaults();

        return await GetAsync<PopularIgdbDto>(queryTrending, pag, IGDBCONSTANTS.URIS.TRENDING);
    }

    public async Task<PagedResult<GenreIgdbDto>> GetGenresAsync(IgdbRequest query, PaginationRequest pag)
    {
        IgdbRequest queryGenres = new IgdbRequest
        {
            Filters = query.Filters,
            OrderBy = query.OrderBy ?? "name",
            Order = query.Order ?? SortOrder.ASC
        };
        //queryGenres.SetDefaults();

        return await GetAsync<GenreIgdbDto>(queryGenres, pag, IGDBCONSTANTS.URIS.GENRES);
    }

    public async Task<PagedResult<T>> GetPlatformsAsync<T>(IgdbRequest query, PaginationRequest pag)
    {
        IgdbRequest queryGenres = new IgdbRequest
        {
            Filters = query.Filters,
            OrderBy = query.OrderBy ?? "name",
            Order = query.Order ?? SortOrder.ASC
        };
        //queryGenres.SetDefaults();

        return await GetAsync<T>(queryGenres, pag, IGDBCONSTANTS.URIS.PLATFORMS);
    }

    public async Task<PagedResult<T>> GetAsync<T>(IgdbRequest query, PaginationRequest pag, string uri)
    {
        // we first check if access token is not set (the server didn't send any queries to igdb yet)
        if (_accessToken is null) await SetAccessToken();

        // set headers, including the access token
        if (!_httpClient.DefaultRequestHeaders.Contains("Client-ID"))
            _httpClient.DefaultRequestHeaders.Add("Client-ID", _clientId);

        if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // parse igdb query body
        IgdbFieldsAttribute attr = typeof(T).GetCustomAttribute<IgdbFieldsAttribute>() ?? throw new Exception("Can't determine IGDB return fields.");

        string fields = $"fields {attr.Fields};";
        string filters = string.IsNullOrEmpty(query.Filters) ? "" : $"where {query.Filters};";
        string sort = string.IsNullOrEmpty(query.OrderBy) ? "" : $"sort {query.OrderBy} {query.Order.SortOrderStr()};";
        string limit = $"limit {pag.PageSize};";
        string offset = pag.Page <= 0 ? "" : $"offset {pag.PageSize * pag.Page};";

        string mainBodyString = $"{fields}{filters}{sort}{limit}{offset}";
        string countBodyString = $"{filters}";

        string fullBodyString = $"query {uri} \"data\" {{ {mainBodyString} }}; query {uri}/count \"count\" {{ {countBodyString} }};";
        var fullBody = new StringContent(fullBodyString, Encoding.UTF8, "text/plain");

        Console.WriteLine($"IGDB Query Body: {fullBodyString}");

        // send POST method to igdb
        HttpResponseMessage response = await _httpClient.PostAsync(IGDBCONSTANTS.URIS.MULTIQUERY, fullBody);

        // if the access token is invalid, we refresh the token and try again (could have expired)
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            await SetAccessToken();

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

            response = await _httpClient.PostAsync(IGDBCONSTANTS.URIS.MULTIQUERY, fullBody);
        }

        // if it's still invalid or there's another error, we throw exceptions
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            throw new Exception("Authorization error while fetching games from IGDB");
        else if (!response.IsSuccessStatusCode)
            throw new Exception($"Unexpected error while fetching games from IGDB\nQuery: {fullBodyString}\nURI: {IGDBCONSTANTS.URIS.MULTIQUERY}\nCode:{response.StatusCode}");

        // if the response is successful, we get the games data as a List of DTO
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = await response.Content.ReadAsStringAsync();

        var igdbResponse = JsonSerializer.Deserialize<List<IgdbResponse>>(json, jsonOptions);

        var data = igdbResponse?.FirstOrDefault(e => e.Name == "data")?.Result;
        var count = igdbResponse?.FirstOrDefault(e => e.Name == "count")?.Count;

        if (data is null || count is null) throw new Exception("IGDB didn't return the expected information. 'data' or 'count' are missing.");

        var igdbData = data?.Deserialize<List<T>>(jsonOptions);
        //Console.WriteLine(await response.Content.ReadAsStringAsync());

        return new PagedResult<T>(igdbData ?? new List<T>(), (int)count, (int)pag.Page!, (int)pag.PageSize!);
    }
}

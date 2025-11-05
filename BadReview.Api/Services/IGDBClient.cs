using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;

using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.Utils;

namespace BadReview.Api.Services;

public class IGDBClient
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

    private async Task GetAccessToken()
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
    public async Task<List<PopularIgdbDto>?> GetTrendingGamesAsync(IgdbRequest query)
    {
        IgdbRequest queryTrending = new IgdbRequest
        {
            Filters = "popularity_type = 3",
            Page = query.Page,
            PageSize = query.PageSize,
            OrderBy = "value"
        };
        queryTrending.SetDefaults();

        return await GetAsync<PopularIgdbDto>(queryTrending, IGDBCONSTANTS.URIS.TRENDING);
    }

    public async Task<List<GenreIgdbDto>?> GetGenresAsync(IgdbRequest query)
    {
        IgdbRequest queryGenres = new IgdbRequest
        {
            Filters = query.Filters,
            Page = query.Page,
            PageSize = query.PageSize,
            OrderBy = query.OrderBy ?? "name",
            Order = query.Order ?? SortOrder.ASC
        };
        queryGenres.SetDefaults();

        return await GetAsync<GenreIgdbDto>(queryGenres, IGDBCONSTANTS.URIS.GENRES);
    }

    public async Task<List<T>?> GetPlatformsAsync<T>(IgdbRequest query)
    {
        IgdbRequest queryGenres = new IgdbRequest
        {
            Filters = query.Filters,
            Page = query.Page,
            PageSize = query.PageSize,
            OrderBy = query.OrderBy ?? "name",
            Order = query.Order ?? SortOrder.ASC
        };
        queryGenres.SetDefaults();

        return await GetAsync<T>(queryGenres, IGDBCONSTANTS.URIS.PLATFORMS);
    }

    public async Task<List<T>?> GetAsync<T>(IgdbRequest query, string uri)
    {
        // we first check if access token is not set (the server didn't send any queries to igdb yet)
        if (_accessToken is null) await GetAccessToken();

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
        string limit = $"limit {query.PageSize};";
        string offset = query.Page <= 0 ? "" : $"offset {query.PageSize * query.Page};";

        string bodyString = $"{fields}{filters}{sort}{limit}{offset}";
        Console.WriteLine($"BodyString: {bodyString}, URI: {uri}");

        //Console.WriteLine(bodyString);

        var body = new StringContent(bodyString, Encoding.UTF8, "text/plain");

        // send POST method to igdb
        HttpResponseMessage response = await _httpClient.PostAsync(uri, body);

        // if the access token is invalid, we refresh the token and try again (could have expired)
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            await GetAccessToken();

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

            response = await _httpClient.PostAsync(uri, body);
        }

        // if it's still invalid or there's another error, we throw exceptions
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            throw new Exception("Authorization error while fetching games from IGDB");
        else if (!response.IsSuccessStatusCode)
            throw new Exception($"Unexpected error while fetching games from IGDB\nQuery: {bodyString}\nURI: {uri}\nCode:{response.StatusCode}");

        // if the response is successful, we get the games data as a List of DTO
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var igdbGames = await response.Content.ReadFromJsonAsync<List<T>>(jsonOptions);

        //Console.WriteLine(await response.Content.ReadAsStringAsync());

        return igdbGames;
    }
}

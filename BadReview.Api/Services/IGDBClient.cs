using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

using BadReview.Api.DTOs.External;
using System.Reflection;
using BadReview.Api.DTOs.Request;

namespace BadReview.Api.Services;

public class IGDBClient
{
    private readonly HttpClient _httpClient;
    private string? _clientId;
    private string? _bearerToken;

    //private static string? _clientId;
    //private static string? _bearerToken;

    public IGDBClient(HttpClient client, IConfiguration config)
    {
        _httpClient = client;

        this._clientId = config["IGDB:ClientId"];
        this._bearerToken = config["IGDB:AccessToken"];

        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_bearerToken))
            throw new ArgumentNullException();
    }

    /*private static async Task GetCredentials()
    {
        _clientId = "k0z2r0lhjxj77ztbr5z45cg9iba8b2";
        _bearerToken = "6ilx1kgnyqnvc4vsfl9lsr3yq9b7n0";
    }*/

    // devolver json formateado con todos los campos incluidos en fields, mapeados a la clase Game
    public async Task<List<T>?> GetGamesAsync<T>(SelectGamesRequest query)
    {
        /*if (_clientId is null || _bearerToken is null) {
            await GetCredentials();
        }*/

        _httpClient.DefaultRequestHeaders.Add("Client-ID", _clientId);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_bearerToken}");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        IgdbFieldsAttribute attr = typeof(T).GetCustomAttribute<IgdbFieldsAttribute>() ?? throw new Exception();
        string fields = attr.Fields;

        //query.Filters = ...;
        string filters = "id = 115";

        /*string bodyString =
            query. is null
                ? $"fields {string.Join(", ", fields)}; limit {query.Limit};"
                : $"fields {string.Join(", ", fields)}; where id = {query.Id};";*/

        string bodyString =
            $"fields {fields}; where {filters}; sort {query.OrderBy} {query.Order}; limit {query.PageSize}; offset {query.PageSize * query.Page};";

        Console.WriteLine(bodyString);

        // Ejemplo de cuerpo de consulta IGDB
        var body = new StringContent(bodyString, Encoding.UTF8, "text/plain");
        
        HttpResponseMessage response = await _httpClient.PostAsync("games", body);
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var igdbGames = await response.Content.ReadFromJsonAsync<List<T>>(jsonOptions);

        Console.WriteLine(await response.Content.ReadAsStringAsync());

        return igdbGames;
    }
}

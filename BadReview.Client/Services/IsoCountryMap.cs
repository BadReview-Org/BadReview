using BadReview.Shared.DTOs.Request;

using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace BadReview.Client.Services;

public class IsoCountryMap : ICountriesIso
{
    public Dictionary<int, IsoCountry>? _lookup { private set; get; }
    private readonly HttpClient _client;
    private readonly NavigationManager _nav;
    private bool alreadyFetched;

    public IsoCountryMap(HttpClient client, NavigationManager nav)
    {
        _client = client;
        _nav = nav;
    }

    public async Task<IsoCountry?> GetAsync(int code)
    {
        if (!alreadyFetched) await LoadAsync();

        if (_lookup is null || _lookup.Count == 0)
            throw new Exception("Couldn't load countries file.");

        return _lookup.TryGetValue(code, out var value) ? value : null;
    }

    public async Task<Dictionary<int, IsoCountry>> GetAsync()
    {
        if (!alreadyFetched) await LoadAsync();

        if (_lookup is null || _lookup.Count == 0)
            throw new Exception("Couldn't load countries file.");

        return _lookup;
    }

    private async Task LoadAsync()
    {
        if (!alreadyFetched)
        {
            var url = _nav.BaseUri + "iso3166.json";

            IsoCountry[]? list =
                await _client.GetFromJsonAsync<IsoCountry[]>(url);

            _lookup = list?.ToDictionary(c => c.Country_code);

            alreadyFetched = true;
        }
    }
}

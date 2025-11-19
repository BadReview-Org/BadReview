using BadReview.Shared.DTOs.Request;

using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using BadReview.Shared.Utils;

namespace BadReview.Client.Services;

public class CredentialsAvailable : ValidatorRules.ICheckAvailables
{
    private readonly HttpClient _client;

    public CredentialsAvailable(HttpClient client)
    {
        _client = client;
    }

    public async Task<bool> UsernameAvailable(string? username)
    {
        if (string.IsNullOrWhiteSpace(username)) return true;


        var response = await _client.PostAsJsonAsync
            (APIUTILS.URIS.USERNAMEAVAILABLE, new UserCheckAvailable(username, null));

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EmailAvailable(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return true;


        var response = await _client.PostAsJsonAsync
            (APIUTILS.URIS.EMAILAVAILABLE, new UserCheckAvailable(null, email));

        return response.IsSuccessStatusCode;
    }
}

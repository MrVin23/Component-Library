using System.Net.Http.Json;
using Shared.Dtos;

namespace Client.Apis;

/// <summary>
/// Client for the antiforgery token endpoint.
/// </summary>
public sealed class ApiAntiforgery(HttpClient http)
{
    private const string BasePath = "api/Antiforgery";

    public async Task<AntiforgeryTokenResponse?> GetRequestTokenAsync(CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<AntiforgeryTokenResponse>($"{BasePath}/token", cancellationToken);
    }
}

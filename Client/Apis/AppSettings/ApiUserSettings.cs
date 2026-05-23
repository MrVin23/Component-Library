using System.Net.Http.Json;
using Shared.Dtos.Users;

namespace Client.Apis.AppSettings;

/// <summary>
/// Client for <c>UserSettingsController</c> routes under <c>api/UserSettings</c>.
/// </summary>
public sealed class ApiUserSettings(HttpClient http)
{
    private const string BasePath = "api/UserSettings";

    /// <summary>GET <c>api/UserSettings/me</c></summary>
    public Task<HttpResponseMessage> GetMineAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/me", cancellationToken);

    /// <summary>PUT <c>api/UserSettings/me</c></summary>
    public Task<HttpResponseMessage> UpdateMineAsync(UpdateUserSettingsRequest request, CancellationToken cancellationToken = default) =>
        http.PutAsJsonAsync($"{BasePath}/me", request, cancellationToken);
}

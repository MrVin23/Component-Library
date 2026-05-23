using System.Net.Http.Json;
using Shared.Dtos.Users;

namespace Client.Apis.UserPermissions;

/// <summary>
/// Client for <c>AuthController</c> routes under <c>api/Auth</c>.
/// </summary>
public sealed class ApiAuth(HttpClient http)
{
    private const string BasePath = "api/Auth";

    /// <summary>POST <c>api/Auth/login</c></summary>
    public Task<HttpResponseMessage> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        http.PostAsJsonAsync($"{BasePath}/login", request, cancellationToken);

    /// <summary>POST <c>api/Auth/signup</c></summary>
    public Task<HttpResponseMessage> SignUpAsync(SignUpRequest request, CancellationToken cancellationToken = default) =>
        http.PostAsJsonAsync($"{BasePath}/signup", request, cancellationToken);

    /// <summary>GET <c>api/Auth/check-username/{username}</c></summary>
    public Task<HttpResponseMessage> CheckUsernameAvailabilityAsync(string username, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/check-username/{Uri.EscapeDataString(username)}", cancellationToken);

    /// <summary>GET <c>api/Auth/check-email/{email}</c></summary>
    public Task<HttpResponseMessage> CheckEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/check-email/{Uri.EscapeDataString(email)}", cancellationToken);

    /// <summary>POST <c>api/Auth/logout</c></summary>
    public Task<HttpResponseMessage> LogoutAsync(CancellationToken cancellationToken = default) =>
        http.PostAsync($"{BasePath}/logout", null, cancellationToken);

    /// <summary>GET <c>api/Auth/me</c></summary>
    public Task<HttpResponseMessage> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/me", cancellationToken);

    /// <summary>GET <c>api/Auth/token-status</c></summary>
    public Task<HttpResponseMessage> GetTokenStatusAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/token-status", cancellationToken);

    /// <summary>POST <c>api/Auth/refresh</c></summary>
    public Task<HttpResponseMessage> RefreshTokenAsync(CancellationToken cancellationToken = default) =>
        http.PostAsync($"{BasePath}/refresh", null, cancellationToken);
}

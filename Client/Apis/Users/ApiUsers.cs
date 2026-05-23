using System.Net.Http.Json;
using Shared.Dtos.Users;

namespace Client.Apis.Users;

/// <summary>
/// Client for <c>UsersController</c> routes under <c>api/Users</c>.
/// </summary>
public sealed class ApiUsers(HttpClient http)
{
    private const string BasePath = "api/Users";

    /// <summary>GET <c>api/Users</c> — list all users (requires auth on server).</summary>
    public Task<HttpResponseMessage> GetAllUsersAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync(BasePath, cancellationToken);

    /// <summary>GET <c>api/Users/{id}</c></summary>
    public Task<HttpResponseMessage> GetUserByIdAsync(int id, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/{id}", cancellationToken);

    /// <summary>GET <c>api/Users/username/{username}</c></summary>
    public Task<HttpResponseMessage> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/username/{Uri.EscapeDataString(username)}", cancellationToken);

    /// <summary>GET <c>api/Users/email/{email}</c></summary>
    public Task<HttpResponseMessage> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/email/{Uri.EscapeDataString(email)}", cancellationToken);

    /// <summary>POST <c>api/Users</c></summary>
    public Task<HttpResponseMessage> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default) =>
        http.PostAsJsonAsync(BasePath, request, cancellationToken);

    /// <summary>PUT <c>api/Users/{id}</c></summary>
    public Task<HttpResponseMessage> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default) =>
        http.PutAsJsonAsync($"{BasePath}/{id}", request, cancellationToken);

    /// <summary>DELETE <c>api/Users/{id}</c></summary>
    public Task<HttpResponseMessage> DeleteUserAsync(int id, CancellationToken cancellationToken = default) =>
        http.DeleteAsync($"{BasePath}/{id}", cancellationToken);

    /// <summary>GET <c>api/Users/exists/username/{username}</c></summary>
    public Task<HttpResponseMessage> UsernameExistsAsync(string username, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/exists/username/{Uri.EscapeDataString(username)}", cancellationToken);

    /// <summary>GET <c>api/Users/exists/email/{email}</c></summary>
    public Task<HttpResponseMessage> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/exists/email/{Uri.EscapeDataString(email)}", cancellationToken);

    /// <summary>GET <c>api/Users/role/{roleId}</c></summary>
    public Task<HttpResponseMessage> GetUsersByRoleAsync(int roleId, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/role/{roleId}", cancellationToken);
}

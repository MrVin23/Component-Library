using System.Net.Http.Json;
using Shared.Dtos.UserPermissions;

namespace Client.Apis.UserPermissions;

/// <summary>
/// Client for <c>SignUpKeyController</c> routes under <c>api/SignUpKey</c>.
/// </summary>
public sealed class ApiSignUpKey(HttpClient http)
{
    private const string BasePath = "api/SignUpKey";

    private static string Paged(string relativePath, int pageNumber, int pageSize) =>
        $"{relativePath}?pageNumber={pageNumber}&pageSize={pageSize}";

    /// <summary>GET <c>api/SignUpKey</c></summary>
    public Task<HttpResponseMessage> GetAllSignUpKeysAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync(BasePath, cancellationToken);

    /// <summary>GET <c>api/SignUpKey/{id}</c></summary>
    public Task<HttpResponseMessage> GetSignUpKeyByIdAsync(int id, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/{id}", cancellationToken);

    /// <summary>GET <c>api/SignUpKey/key/{key}</c></summary>
    public Task<HttpResponseMessage> GetSignUpKeyByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/key/{Uri.EscapeDataString(key)}", cancellationToken);

    /// <summary>GET <c>api/SignUpKey/paged</c></summary>
    public Task<HttpResponseMessage> GetSignUpKeysPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default) =>
        http.GetAsync(Paged($"{BasePath}/paged", pageNumber, pageSize), cancellationToken);

    /// <summary>GET <c>api/SignUpKey/active</c></summary>
    public Task<HttpResponseMessage> GetActiveSignUpKeysAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/active", cancellationToken);

    /// <summary>GET <c>api/SignUpKey/expired</c></summary>
    public Task<HttpResponseMessage> GetExpiredSignUpKeysAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/expired", cancellationToken);

    /// <summary>GET <c>api/SignUpKey/validate/{key}</c></summary>
    public Task<HttpResponseMessage> ValidateSignUpKeyAsync(string key, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/validate/{Uri.EscapeDataString(key)}", cancellationToken);

    /// <summary>POST <c>api/SignUpKey</c></summary>
    public Task<HttpResponseMessage> CreateSignUpKeyAsync(CreateSignUpKeyRequest request, CancellationToken cancellationToken = default) =>
        http.PostAsJsonAsync(BasePath, request, cancellationToken);

    /// <summary>PUT <c>api/SignUpKey/{id}</c></summary>
    public Task<HttpResponseMessage> UpdateSignUpKeyAsync(int id, UpdateSignUpKeyRequest request, CancellationToken cancellationToken = default) =>
        http.PutAsJsonAsync($"{BasePath}/{id}", request, cancellationToken);

    /// <summary>DELETE <c>api/SignUpKey/{id}</c></summary>
    public Task<HttpResponseMessage> DeleteSignUpKeyAsync(int id, CancellationToken cancellationToken = default) =>
        http.DeleteAsync($"{BasePath}/{id}", cancellationToken);

    /// <summary>DELETE <c>api/SignUpKey/all</c></summary>
    public Task<HttpResponseMessage> DeleteAllSignUpKeysAsync(CancellationToken cancellationToken = default) =>
        http.DeleteAsync($"{BasePath}/all", cancellationToken);
}

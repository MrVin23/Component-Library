namespace Client.Apis.UserPermissions;

/// <summary>
/// Client for <c>TestAuthController</c> routes under <c>api/test-auth</c>.
/// </summary>
public sealed class ApiTest(HttpClient http)
{
    private const string BasePath = "api/test-auth";

    /// <summary>GET <c>api/Test/permission/{permissionName}</c></summary>
    public Task<HttpResponseMessage> TestPermissionAsync(string permissionName, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/permission/{Uri.EscapeDataString(permissionName)}", cancellationToken);

    /// <summary>GET <c>api/test-auth/can-access/read-only</c></summary>
    public Task<HttpResponseMessage> TestReadOnlyAccessAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/can-access/read-only", cancellationToken);

    /// <summary>GET <c>api/test-auth/can-access/read-write</c></summary>
    public Task<HttpResponseMessage> TestReadWriteAccessAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/can-access/read-write", cancellationToken);

    /// <summary>GET <c>api/test-auth/can-access/all-permissions</c></summary>
    public Task<HttpResponseMessage> TestAllPermissionsAccessAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/can-access/all-permissions", cancellationToken);

    /// <summary>GET <c>api/test-auth/my-permissions</c></summary>
    public Task<HttpResponseMessage> GetMyPermissionsAsync(CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/my-permissions", cancellationToken);
}

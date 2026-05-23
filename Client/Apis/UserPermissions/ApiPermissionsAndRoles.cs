using System.Net.Http.Json;
using Shared.Dtos.UserPermissions;

namespace Client.Apis.UserPermissions;

/// <summary>
/// Client for <c>PermissionsAndRolesController</c> routes under <c>api/PermissionsAndRoles</c>.
/// </summary>
public sealed class ApiPermissionsAndRoles(HttpClient http)
{
    private const string BasePath = "api/PermissionsAndRoles";

    private static string Paged(string relativePath, int pageNumber, int pageSize) =>
        $"{relativePath}?pageNumber={pageNumber}&pageSize={pageSize}";

    /// <summary>GET <c>api/PermissionsAndRoles/role-permissions</c></summary>
    public Task<HttpResponseMessage> GetRolePermissionsAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default) =>
        http.GetAsync(Paged($"{BasePath}/role-permissions", pageNumber, pageSize), cancellationToken);

    /// <summary>GET <c>api/PermissionsAndRoles/roles/{roleId}/permissions</c></summary>
    public Task<HttpResponseMessage> GetPermissionsByRoleAsync(int roleId, CancellationToken cancellationToken = default) =>
        http.GetAsync($"{BasePath}/roles/{roleId}/permissions", cancellationToken);

    /// <summary>PUT <c>api/PermissionsAndRoles/roles/{roleId}/permissions</c></summary>
    public Task<HttpResponseMessage> SetRolePermissionsAsync(int roleId, SetPermissionsRequest request, CancellationToken cancellationToken = default) =>
        http.PutAsJsonAsync($"{BasePath}/roles/{roleId}/permissions", request, cancellationToken);

    /// <summary>DELETE <c>api/PermissionsAndRoles/roles/{roleId}/permissions/{permissionId}</c></summary>
    public Task<HttpResponseMessage> RemovePermissionFromRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken = default) =>
        http.DeleteAsync($"{BasePath}/roles/{roleId}/permissions/{permissionId}", cancellationToken);

    /// <summary>GET <c>api/PermissionsAndRoles/roles</c></summary>
    public Task<HttpResponseMessage> GetRolesAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default) =>
        http.GetAsync(Paged($"{BasePath}/roles", pageNumber, pageSize), cancellationToken);

    /// <summary>GET <c>api/PermissionsAndRoles/users</c></summary>
    public Task<HttpResponseMessage> GetUsersWithRolesAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default) =>
        http.GetAsync(Paged($"{BasePath}/users", pageNumber, pageSize), cancellationToken);

    /// <summary>GET <c>api/PermissionsAndRoles/user-roles</c></summary>
    public Task<HttpResponseMessage> GetUserRolesAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default) =>
        http.GetAsync(Paged($"{BasePath}/user-roles", pageNumber, pageSize), cancellationToken);

    /// <summary>DELETE <c>api/PermissionsAndRoles/users/{userId}/roles/{roleId}</c></summary>
    public Task<HttpResponseMessage> RemoveUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default) =>
        http.DeleteAsync($"{BasePath}/users/{userId}/roles/{roleId}", cancellationToken);

    /// <summary>POST <c>api/PermissionsAndRoles/users/{userId}/roles/{roleId}</c></summary>
    public Task<HttpResponseMessage> AssignRoleToUserAsync(int userId, int roleId, CancellationToken cancellationToken = default) =>
        http.PostAsync($"{BasePath}/users/{userId}/roles/{roleId}", null, cancellationToken);

    /// <summary>POST <c>api/PermissionsAndRoles/roles</c></summary>
    public Task<HttpResponseMessage> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default) =>
        http.PostAsJsonAsync($"{BasePath}/roles", request, cancellationToken);

    /// <summary>PUT <c>api/PermissionsAndRoles/roles/{roleId}</c></summary>
    public Task<HttpResponseMessage> UpdateRoleAsync(int roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default) =>
        http.PutAsJsonAsync($"{BasePath}/roles/{roleId}", request, cancellationToken);

    /// <summary>DELETE <c>api/PermissionsAndRoles/roles/{roleId}</c></summary>
    public Task<HttpResponseMessage> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default) =>
        http.DeleteAsync($"{BasePath}/roles/{roleId}", cancellationToken);

    /// <summary>GET <c>api/PermissionsAndRoles/permissions</c></summary>
    public Task<HttpResponseMessage> GetPermissionsAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default) =>
        http.GetAsync(Paged($"{BasePath}/permissions", pageNumber, pageSize), cancellationToken);

    /// <summary>POST <c>api/PermissionsAndRoles/permissions</c></summary>
    public Task<HttpResponseMessage> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken cancellationToken = default) =>
        http.PostAsJsonAsync($"{BasePath}/permissions", request, cancellationToken);

    /// <summary>PUT <c>api/PermissionsAndRoles/permissions/{permissionId}</c></summary>
    public Task<HttpResponseMessage> UpdatePermissionAsync(int permissionId, UpdatePermissionRequest request, CancellationToken cancellationToken = default) =>
        http.PutAsJsonAsync($"{BasePath}/permissions/{permissionId}", request, cancellationToken);

    /// <summary>DELETE <c>api/PermissionsAndRoles/permissions/{permissionId}</c></summary>
    public Task<HttpResponseMessage> DeletePermissionAsync(int permissionId, CancellationToken cancellationToken = default) =>
        http.DeleteAsync($"{BasePath}/permissions/{permissionId}", cancellationToken);

    /// <summary>POST <c>api/PermissionsAndRoles/roles/{roleId}/permissions/{permissionId}</c></summary>
    public Task<HttpResponseMessage> AssignPermissionToRoleAsync(int roleId, int permissionId, CancellationToken cancellationToken = default) =>
        http.PostAsync($"{BasePath}/roles/{roleId}/permissions/{permissionId}", null, cancellationToken);
}

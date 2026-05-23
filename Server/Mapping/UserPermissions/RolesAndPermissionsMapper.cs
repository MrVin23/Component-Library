using Server.Models.UserPermissions;
using Shared.Dtos.UserPermissions;

namespace Server.Mapping.UserPermissions;

public static class RolesAndPermissionsMapper
{
    public static Role ToNewRole(CreateRoleRequest request)
    {
        var utc = DateTime.UtcNow;
        return new Role
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            CreatedAt = utc,
            UpdatedAt = utc
        };
    }

    public static Permission ToNewPermission(CreatePermissionRequest request)
    {
        var utc = DateTime.UtcNow;
        return new Permission
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            CreatedAt = utc,
            UpdatedAt = utc
        };
    }

    public static RolePermission ToNewRolePermission(int roleId, int permissionId)
    {
        var utc = DateTime.UtcNow;
        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = utc,
            UpdatedAt = utc
        };
    }

    public static PermissionResponse ToPermissionResponse(Permission p) =>
        new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedAt = p.CreatedAt.UtcDateTime,
            UpdatedAt = p.UpdatedAt.UtcDateTime
        };

    public static RolePermissionResponse ToRolePermissionResponse(RolePermission rp) =>
        new()
        {
            Id = rp.Id,
            RoleId = rp.RoleId,
            RoleName = rp.Role?.Name ?? string.Empty,
            PermissionId = rp.PermissionId,
            PermissionName = rp.Permission?.Name ?? string.Empty,
            CreatedAt = rp.CreatedAt.UtcDateTime,
            UpdatedAt = rp.UpdatedAt.UtcDateTime
        };

    public static RoleWithPermissionsResponse ToRoleWithPermissionsResponse(
        Role role,
        IEnumerable<Permission> permissions) =>
        new()
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt.UtcDateTime,
            UpdatedAt = role.UpdatedAt.UtcDateTime,
            Permissions = permissions.Select(ToPermissionResponse).ToList()
        };

    public static RoleWithPermissionsResponse ToRoleWithPermissionsResponse(Role role) =>
        ToRoleWithPermissionsResponse(role, Array.Empty<Permission>());

    public static UserRoleResponse ToUserRoleResponse(UserRole ur) =>
        new()
        {
            Id = ur.Id,
            UserId = ur.UserId,
            Username = ur.User?.Username ?? string.Empty,
            FirstName = ur.User?.FirstName ?? string.Empty,
            LastName = ur.User?.LastName ?? string.Empty,
            Email = ur.User?.Email ?? string.Empty,
            RoleId = ur.RoleId,
            RoleName = ur.Role?.Name ?? string.Empty,
            CreatedAt = ur.CreatedAt.UtcDateTime,
            UpdatedAt = ur.UpdatedAt.UtcDateTime
        };

    public static UserWithRolesResponse ToUserWithRolesResponse(
        UserRole firstUserRole,
        List<RoleWithPermissionsResponse> roles) =>
        new()
        {
            UserId = firstUserRole.UserId,
            Username = firstUserRole.User?.Username ?? string.Empty,
            Email = firstUserRole.User?.Email ?? string.Empty,
            FirstName = firstUserRole.User?.FirstName ?? string.Empty,
            LastName = firstUserRole.User?.LastName ?? string.Empty,
            Roles = roles
        };

    public static RolePermissionResponse ToRolePermissionAssignmentResponse(
        int roleId,
        string? roleName,
        int permissionId,
        string? permissionName)
    {
        var utc = DateTime.UtcNow;
        return new RolePermissionResponse
        {
            RoleId = roleId,
            RoleName = roleName ?? string.Empty,
            PermissionId = permissionId,
            PermissionName = permissionName ?? string.Empty,
            CreatedAt = utc,
            UpdatedAt = utc
        };
    }
}

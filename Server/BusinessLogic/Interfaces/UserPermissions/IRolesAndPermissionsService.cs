using Server.Models;
using Server.Models.UserPermissions;
using Shared.Dtos;
using Shared.Dtos.UserPermissions;

namespace Server.BusinessLogic.Interfaces.UserPermissions
{
    public interface IRolesAndPermissionsService
    {
        // Role operations
        Task<Role?> GetRoleByIdAsync(int id);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<PagedResponse<Role>> GetRolesPagedAsync(PaginationParameters parameters);
        Task<Role> CreateRoleAsync(CreateRoleRequest request);
        Task<Role> UpdateRoleAsync(int id, UpdateRoleRequest request);
        Task<bool> DeleteRoleAsync(int id);

        // Permission operations
        Task<Permission?> GetPermissionByIdAsync(int id);
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        Task<PagedResponse<Permission>> GetPermissionsPagedAsync(PaginationParameters parameters);
        Task<Permission> CreatePermissionAsync(CreatePermissionRequest request);
        Task<Permission> UpdatePermissionAsync(int id, UpdatePermissionRequest request);
        Task<bool> DeletePermissionAsync(int id);

        // Role-Permission operations
        Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(int roleId);
        Task AssignPermissionToRoleAsync(int roleId, int permissionId);
        Task RemovePermissionFromRoleAsync(int roleId, int permissionId);
        Task SetRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds);
        Task<bool> RoleHasPermissionAsync(int roleId, int permissionId);
        Task<PagedResponse<RolePermission>> GetRolePermissionsPagedAsync(PaginationParameters parameters);
    }
}

using Server.Models.UserPermissions;
using Server.Repositories.Interfaces;

namespace Server.Repositories.Interfaces.UserPermissions
{
    public interface IRolePermissionRepository : IGenericRepository<RolePermission>
    {
        Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleIdAsync(int roleId);
        Task<IEnumerable<RolePermission>> GetRolePermissionsByPermissionIdAsync(int permissionId);
        Task<RolePermission?> GetRolePermissionAsync(int roleId, int permissionId);
        Task<bool> RoleHasPermissionAsync(int roleId, int permissionId);
        Task RemoveRolePermissionAsync(int roleId, int permissionId);
        Task UpdateRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds);
    }
}

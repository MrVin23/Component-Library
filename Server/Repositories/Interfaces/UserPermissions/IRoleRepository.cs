using Server.Models.UserPermissions;
using Server.Repositories.Interfaces;

namespace Server.Repositories.Interfaces.UserPermissions
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name);
        Task<IEnumerable<Role>> GetRolesByPermissionAsync(int permissionId);
        Task<bool> RoleNameExistsAsync(string name);
        Task<Role?> GetRoleWithPermissionsAsync(int roleId);
    }
}

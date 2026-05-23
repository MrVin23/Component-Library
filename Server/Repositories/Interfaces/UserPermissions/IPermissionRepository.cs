using Server.Models.UserPermissions;
using Server.Repositories.Interfaces;

namespace Server.Repositories.Interfaces.UserPermissions
{
    public interface IPermissionRepository : IGenericRepository<Permission>
    {
        Task<Permission?> GetByNameAsync(string name);
        Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(int roleId);
        Task<bool> PermissionNameExistsAsync(string name);
    }
}

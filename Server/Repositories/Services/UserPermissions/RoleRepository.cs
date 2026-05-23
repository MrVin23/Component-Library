using Microsoft.EntityFrameworkCore;
using Server.Models.UserPermissions;
using Server.Repositories.Interfaces.UserPermissions;
using Server.Repositories.Services;
using Server.Database;

namespace Server.Repositories.Services.UserPermissions
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<IEnumerable<Role>> GetRolesByPermissionAsync(int permissionId)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Where(r => r.RolePermissions.Any(rp => rp.PermissionId == permissionId))
                .ToListAsync();
        }

        public async Task<bool> RoleNameExistsAsync(string name)
        {
            return await _dbSet.AnyAsync(r => r.Name == name);
        }

        public async Task<Role?> GetRoleWithPermissionsAsync(int roleId)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public override async Task<Role?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}

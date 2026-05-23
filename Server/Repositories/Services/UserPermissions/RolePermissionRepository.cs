using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;
using Server.Models.UserPermissions;
using Server.Repositories.Interfaces.UserPermissions;
using Server.Repositories.Services;
using Server.Database;

namespace Server.Repositories.Services.UserPermissions
{
    public class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
    {
        public RolePermissionRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleIdAsync(int roleId)
        {
            return await _dbSet
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RolePermission>> GetRolePermissionsByPermissionIdAsync(int permissionId)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Where(rp => rp.PermissionId == permissionId)
                .ToListAsync();
        }

        public async Task<RolePermission?> GetRolePermissionAsync(int roleId, int permissionId)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        }

        public async Task<bool> RoleHasPermissionAsync(int roleId, int permissionId)
        {
            return await _dbSet.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        }

        public async Task RemoveRolePermissionAsync(int roleId, int permissionId)
        {
            var rolePermission = await GetRolePermissionAsync(roleId, permissionId);
            if (rolePermission != null)
            {
                await DeleteAsync(rolePermission);
            }
        }

        public async Task UpdateRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds)
        {
            // Remove existing permissions for the role
            var existingPermissions = await GetRolePermissionsByRoleIdAsync(roleId);
            await DeleteRangeAsync(existingPermissions);

            // Add new permissions
            var newRolePermissions = permissionIds.Select(permissionId => new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });

            await AddRangeAsync(newRolePermissions);
        }

        public override async Task<RolePermission?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .FirstOrDefaultAsync(rp => rp.Id == id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;
using Server.Repositories.Interfaces.UserPermissions;
using Server.Models.UserPermissions;
using Server.Repositories.Services;
using Server.Database;

namespace Server.Repositories.Services.UserPermissions
{
    public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
    {
        public PermissionRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<Permission?> GetByNameAsync(string name)
        {
            return await _dbSet
                .Include(p => p.RolePermissions)
                .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(int roleId)
        {
            return await _dbSet
                .Include(p => p.RolePermissions)
                .ThenInclude(rp => rp.Role)
                .Where(p => p.RolePermissions.Any(rp => rp.RoleId == roleId))
                .ToListAsync();
        }

        public async Task<bool> PermissionNameExistsAsync(string name)
        {
            return await _dbSet.AnyAsync(p => p.Name == name);
        }

        public override async Task<Permission?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.RolePermissions)
                .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}

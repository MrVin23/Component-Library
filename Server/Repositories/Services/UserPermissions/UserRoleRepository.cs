using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;
using Server.Models.UserPermissions;
using Server.Repositories.Interfaces.UserPermissions;
using Server.Repositories.Services;
using Server.Database;

namespace Server.Repositories.Services.UserPermissions
{
    public class UserRoleRepository : GenericRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesByRoleIdAsync(int roleId)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Where(ur => ur.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<UserRole?> GetUserRoleAsync(int userId, int roleId)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task<bool> UserHasRoleAsync(int userId, int roleId)
        {
            return await _dbSet.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task RemoveUserRoleAsync(int userId, int roleId)
        {
            var userRole = await GetUserRoleAsync(userId, roleId);
            if (userRole != null)
            {
                await DeleteAsync(userRole);
            }
        }

        public override async Task<UserRole?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.Id == id);
        }
    }
}

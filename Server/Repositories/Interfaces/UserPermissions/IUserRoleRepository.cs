using Server.Models.UserPermissions;
using Server.Repositories.Interfaces;

namespace Server.Repositories.Interfaces.UserPermissions
{
    public interface IUserRoleRepository : IGenericRepository<UserRole>
    {
        Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(int userId);
        Task<IEnumerable<UserRole>> GetUserRolesByRoleIdAsync(int roleId);
        Task<UserRole?> GetUserRoleAsync(int userId, int roleId);
        Task<bool> UserHasRoleAsync(int userId, int roleId);
        Task RemoveUserRoleAsync(int userId, int roleId);
    }
}

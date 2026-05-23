using Microsoft.EntityFrameworkCore;
using Server.Repositories.Interfaces.UserPermissions;
using Server.Models.UserPermissions;
using Server.Database;

namespace Server.Repositories.Services.UserPermissions
{
    public class SignUpKeyRepository : GenericRepository<SignUpKey>, ISignUpKeyRepository
    {
        public SignUpKeyRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<SignUpKey?> GetByKeyAsync(string key)
        {
            return await _dbSet.FirstOrDefaultAsync(sk => sk.Key == key);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _dbSet.AnyAsync(sk => sk.Key == key);
        }

        public async Task<IEnumerable<SignUpKey>> GetExpiredKeysAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(sk => sk.ExpiresAt < now)
                .ToListAsync();
        }

        public async Task<IEnumerable<SignUpKey>> GetActiveKeysAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(sk => sk.ExpiresAt >= now)
                .ToListAsync();
        }
    }
}

using Server.Models.UserPermissions;
using Server.Repositories.Interfaces;

namespace Server.Repositories.Interfaces.UserPermissions
{
    public interface ISignUpKeyRepository : IGenericRepository<SignUpKey>
    {
        Task<SignUpKey?> GetByKeyAsync(string key);
        Task<bool> KeyExistsAsync(string key);
        Task<IEnumerable<SignUpKey>> GetExpiredKeysAsync();
        Task<IEnumerable<SignUpKey>> GetActiveKeysAsync();
    }
}

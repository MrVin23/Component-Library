using Server.Models;
using Server.Models.UserPermissions;
using Shared.Dtos.UserPermissions;

namespace Server.BusinessLogic.Interfaces.UserPermissions
{
    public interface ISignUpKeyService
    {
        Task<SignUpKey?> GetByIdAsync(int id);
        Task<SignUpKey?> GetByKeyAsync(string key);
        Task<IEnumerable<SignUpKey>> GetAllAsync();
        Task<PagedResponse<SignUpKey>> GetPagedAsync(PaginationParameters parameters);
        Task<SignUpKey> CreateAsync(CreateSignUpKeyRequest request);
        Task<SignUpKey> UpdateAsync(int id, UpdateSignUpKeyRequest request);
        Task<bool> DeleteAsync(int id);
        Task<int> DeleteAllAsync();
        Task<bool> IsKeyValidAsync(string key);
        Task<IEnumerable<SignUpKey>> GetActiveKeysAsync();
        Task<IEnumerable<SignUpKey>> GetExpiredKeysAsync();
    }
}

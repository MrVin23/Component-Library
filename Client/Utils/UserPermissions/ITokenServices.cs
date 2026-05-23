using System.Security.Claims;

namespace Client.Utils.UserPermissions
{
    public interface ITokenServices
    {
        IEnumerable<Claim> ParseClaimsFromJwt(string jwt);
        Task StoreRoleToken(string roleToken);
        Task<string?> GetRoleToken();
        Task RemoveRoleToken();
    }
}
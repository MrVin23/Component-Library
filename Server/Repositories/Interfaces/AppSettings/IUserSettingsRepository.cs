using Server.Models.AppSettings;
using Server.Repositories.Interfaces;

namespace Server.Repositories.Interfaces.AppSettings
{
    public interface IUserSettingsRepository : IGenericRepository<UserSettings>
    {
        Task<UserSettings?> GetByUserIdAsync(int userId);
    }
}

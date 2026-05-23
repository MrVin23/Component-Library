using Microsoft.EntityFrameworkCore;
using Server.Database;
using Server.Models.AppSettings;
using Server.Repositories.Interfaces.AppSettings;
using Server.Repositories.Services;

namespace Server.Repositories.Services.AppSettings
{
    public class UserSettingsRepository : GenericRepository<UserSettings>, IUserSettingsRepository
    {
        public UserSettingsRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<UserSettings?> GetByUserIdAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(us => us.UserId == userId);
        }
    }
}

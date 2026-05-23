using Server.Models.Users;

namespace Server.Models.AppSettings
{
    public class UserSettings : BaseModel
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public bool DarkMode { get; set; } = true;
    }
}

namespace Shared.Dtos.Users
{
    public class UserSettingsResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool DarkMode { get; set; }
    }

    public class UpdateUserSettingsRequest
    {
        public bool DarkMode { get; set; }
    }
}

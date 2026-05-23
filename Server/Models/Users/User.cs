using System.ComponentModel.DataAnnotations;
using Server.Models.AppSettings;
using Server.Models.UserPermissions;

namespace Server.Models.Users
{
    public class User : BaseModel
    {
        public string? Username { get; set; } // Required
        public string? FirstName { get; set; } // Optional
        public string? LastName { get; set; } // Optional
        public string? Email { get; set; } // Required
        public string? Password { get; set; } // Stored as BCrypt hash in database
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public UserSettings? UserSettings { get; set; }
    }
}
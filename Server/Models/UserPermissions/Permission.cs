using Server.Models;

namespace Server.Models.UserPermissions
{
    public class Permission : BaseModel
    {
        public string Name { get; set; } = string.Empty; // e.g., "CanAccessAdminPanel", "CanEditPosts"
        public string Description { get; set; } = string.Empty;
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}

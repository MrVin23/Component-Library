namespace Server.Models.UserPermissions
{
    public class SignUpKey : BaseModel
    {
        public string Key { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
namespace Shared.Dtos.UserPermissions
{
    /// <summary>
    /// Request model for creating a sign-up key
    /// </summary>
    public class CreateSignUpKeyRequest
    {
        /// <summary>
        /// Optional: The key value. If not provided, a unique key will be generated.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Expiration date and time for the key
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Request model for updating a sign-up key
    /// </summary>
    public class UpdateSignUpKeyRequest
    {
        /// <summary>
        /// Optional: New key value
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Optional: New expiration date and time
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Response model for sign-up key data
    /// </summary>
    public class SignUpKeyResponse
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsExpired { get; set; }
        public bool IsActive { get; set; }
    }
}

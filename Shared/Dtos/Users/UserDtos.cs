namespace Shared.Dtos.Users
{
    /// <summary>
    /// Request model for user sign up (public registration)
    /// </summary>
    public class SignUpRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string SignUpKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for creating a user (admin operation)
    /// </summary>
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for updating a user
    /// </summary>
    public class UpdateUserRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Password { get; set; }
    }

    /// <summary>
    /// Response model for user data
    /// </summary>
    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Request model for user login
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for role data
    /// </summary>
    public class RoleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for login/authentication
    /// </summary>
    public class LoginResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public List<RoleResponse> RoleDetails { get; set; } = new List<RoleResponse>();
    }

    /// <summary>
    /// Response containing authentication token status information
    /// </summary>
    public class TokenStatusResponse
    {
        /// <summary>
        /// Whether the user is currently authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }
        
        /// <summary>
        /// The authenticated username
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// When the token expires (UTC)
        /// </summary>
        public DateTimeOffset? ExpiresUtc { get; set; }
        
        /// <summary>
        /// When the token was issued (UTC)
        /// </summary>
        public DateTimeOffset? IssuedUtc { get; set; }
        
        /// <summary>
        /// Time remaining until token expires
        /// </summary>
        public TimeSpan? TimeRemaining { get; set; }
        
        /// <summary>
        /// True if the token is expiring soon (within 10 minutes)
        /// Use this to trigger a refresh
        /// </summary>
        public bool IsExpiringSoon { get; set; }
    }

    /// <summary>
    /// Response model for sign up operation
    /// </summary>
    public class SignUpResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

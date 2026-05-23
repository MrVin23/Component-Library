using Shared.Dtos;
using Shared.Dtos.Users;

namespace Client.Utils.UserPermissions
{
    public interface IAuthService
    {
        /// <summary>
        /// Login with username and password
        /// </summary>
        Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request);

        /// <summary>
        /// Register a new user account
        /// </summary>
        Task<ApiResponse<SignUpResponse>?> SignUpAsync(SignUpRequest request);

        /// <summary>
        /// Check if a username is available for registration
        /// </summary>
        Task<ApiResponse<bool>?> CheckUsernameAvailabilityAsync(string username);

        /// <summary>
        /// Check if an email is available for registration
        /// </summary>
        Task<ApiResponse<bool>?> CheckEmailAvailabilityAsync(string email);

        /// <summary>
        /// Logout the current user
        /// </summary>
        Task<ApiResponse<object>?> LogoutAsync();

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        Task<ApiResponse<LoginResponse>?> GetCurrentUserAsync();

        /// <summary>
        /// Get the current token status including expiration info
        /// </summary>
        Task<ApiResponse<TokenStatusResponse>?> GetTokenStatusAsync();

        /// <summary>
        /// Refresh the authentication token (extends session)
        /// </summary>
        Task<ApiResponse<TokenStatusResponse>?> RefreshTokenAsync();

        /// <summary>
        /// Refresh client auth state from the server session/cookie.
        /// </summary>
        Task RefreshClientAuthStateAsync();

        /// <summary>
        /// Check whether current user has a specific role.
        /// </summary>
        Task<bool> IsInRoleAsync(string role);

        /// <summary>
        /// Check whether current user has any of the provided roles.
        /// </summary>
        Task<bool> IsInAnyRoleAsync(params string[] roles);
    }
}


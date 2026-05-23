using Server.Models.Users;
using Shared.Dtos.Users;

namespace Server.BusinessLogic.Interfaces.UserPermissions
{
    /// <summary>
    /// Service interface for user sign up operations
    /// </summary>
    public interface ISignUpService
    {
        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="request">The sign up request containing user details</param>
        /// <returns>The created user</returns>
        /// <exception cref="FluentValidation.ValidationException">Thrown when validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when username or email already exists</exception>
        Task<User> SignUpAsync(SignUpRequest request);

        /// <summary>
        /// Checks if a username is available for registration
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <returns>True if the username is available, false otherwise</returns>
        Task<bool> IsUsernameAvailableAsync(string username);

        /// <summary>
        /// Checks if an email is available for registration
        /// </summary>
        /// <param name="email">The email to check</param>
        /// <returns>True if the email is available, false otherwise</returns>
        Task<bool> IsEmailAvailableAsync(string email);
    }
}

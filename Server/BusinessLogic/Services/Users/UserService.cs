using Microsoft.Extensions.Logging;
using Server.Repositories.Services.UserPermissions;
using Server.Repositories.Interfaces.Users;
using Server.Interfaces;
using Server.Models.Users;
using Shared.Dtos.Users;
using Server.Repositories.Interfaces.UserPermissions;
using Server.Repositories;
using Server.Utils.UserPermissions;

namespace Server.BusinessLogic.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly DuplicateChecker _duplicateChecker;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            DuplicateChecker duplicateChecker,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _duplicateChecker = duplicateChecker;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            var userToCheck = new User
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = PasswordHelper.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var duplicateResults = await _duplicateChecker.CheckForDuplicate(userToCheck,
                u => u.Username!,
                u => u.Email!
            );

            var duplicateFields = duplicateResults
                .Where(r => r.IsDuplicate)
                .Select(r => r.DuplicateField)
                .ToList();

            if (duplicateFields.Any())
            {
                var fieldList = string.Join(" and ", duplicateFields);
                _logger.LogWarning("Create user rejected: duplicate fields {DuplicateFields}", fieldList);
                throw new InvalidOperationException($"The following fields already exist: {fieldList}");
            }

            return await _userRepository.AddAsync(userToCheck);
        }

        public async Task<User> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Update user failed: user {UserId} not found", id);
                throw new ArgumentException($"User with ID {id} not found.");
            }

            var tempUser = new User
            {
                Username = !string.IsNullOrEmpty(request.Username) ? request.Username : user.Username,
                Email = !string.IsNullOrEmpty(request.Email) ? request.Email : user.Email,
                FirstName = !string.IsNullOrEmpty(request.FirstName) ? request.FirstName : user.FirstName,
                LastName = !string.IsNullOrEmpty(request.LastName) ? request.LastName : user.LastName
            };

            var propertiesToCheck = new List<System.Linq.Expressions.Expression<Func<User, object>>>();

            if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
            {
                propertiesToCheck.Add(u => u.Username!);
            }

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                propertiesToCheck.Add(u => u.Email!);
            }

            if (propertiesToCheck.Any())
            {
                await _duplicateChecker.CheckForDuplicate(tempUser, propertiesToCheck.ToArray());

                if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
                {
                    if (await _userRepository.UsernameExistsAsync(request.Username))
                    {
                        _logger.LogWarning("Update user rejected: username already exists for user {UserId}", id);
                        throw new InvalidOperationException($"Username '{request.Username}' already exists.");
                    }
                }

                if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
                {
                    if (await _userRepository.EmailExistsAsync(request.Email))
                    {
                        _logger.LogWarning("Update user rejected: email already exists for user {UserId}", id);
                        throw new InvalidOperationException($"Email '{request.Email}' already exists.");
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.Username))
                user.Username = request.Username;

            if (!string.IsNullOrEmpty(request.Email))
                user.Email = request.Email;

            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.Password))
                user.Password = PasswordHelper.HashPassword(request.Password);

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(user);
            return true;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _userRepository.UsernameExistsAsync(username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId)
        {
            return await _userRepository.GetUsersByRoleAsync(roleId);
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                _logger.LogDebug("Validate user: rejected (same response for invalid user or credentials)");
                return null;
            }

            if (!PasswordHelper.VerifyPassword(password, user.Password))
            {
                _logger.LogDebug("Validate user: rejected (same response for invalid user or credentials)");
                return null;
            }

            return user;
        }
    }
}

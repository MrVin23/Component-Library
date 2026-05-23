using FluentValidation;
using Microsoft.Extensions.Logging;
using Server.Models.Users;
using Server.Utils.UserPermissions;
using Server.Repositories.Interfaces.Users;
using Shared.Dtos.UserPermissions;
using Server.BusinessLogic.Interfaces.UserPermissions;
using Server.Mapping.UserPermissions;
using Shared.Dtos.Users;

namespace Server.BusinessLogic.Services.UserPermissions
{
    public class SignUpService : ISignUpService
    {
        private readonly IUserRepository _userRepository;
        private readonly IValidator<SignUpRequest> _signUpValidator;
        private readonly ILogger<SignUpService> _logger;

        public SignUpService(
            IUserRepository userRepository,
            IValidator<SignUpRequest> signUpValidator,
            ILogger<SignUpService> logger)
        {
            _userRepository = userRepository;
            _signUpValidator = signUpValidator;
            _logger = logger;
        }

        public async Task<User> SignUpAsync(SignUpRequest request)
        {
            request.Username = request.Username.Trim();
            request.Email = request.Email.Trim().ToLowerInvariant();
            request.FirstName = request.FirstName.Trim();
            request.LastName = request.LastName.Trim();

            var validationResult = await _signUpValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            if (await _userRepository.UsernameExistsAsync(request.Username))
            {
                _logger.LogWarning("Sign-up rejected: username already taken");
                throw new InvalidOperationException($"Username '{request.Username}' is already taken.");
            }

            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                _logger.LogWarning("Sign-up rejected: email already registered");
                throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
            }

            var user = SignUpMapper.ToNewUser(request, PasswordHelper.HashPassword(request.Password));

            return await _userRepository.AddAsync(user);
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            return !await _userRepository.UsernameExistsAsync(username.Trim());
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return !await _userRepository.EmailExistsAsync(email.Trim().ToLowerInvariant());
        }
    }
}

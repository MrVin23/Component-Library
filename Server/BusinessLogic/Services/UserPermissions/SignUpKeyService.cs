using FluentValidation;
using Microsoft.Extensions.Logging;
using Server.BusinessLogic.Interfaces.UserPermissions;
using Server.Models;
using Server.Models.UserPermissions;
using Server.Repositories.Interfaces.UserPermissions;
using Server.Mapping.UserPermissions;
using Shared.Dtos.UserPermissions;

namespace Server.BusinessLogic.Services.UserPermissions
{
    public class SignUpKeyService : ISignUpKeyService
    {
        private readonly ISignUpKeyRepository _signUpKeyRepository;
        private readonly IValidator<CreateSignUpKeyRequest> _createValidator;
        private readonly IValidator<UpdateSignUpKeyRequest> _updateValidator;
        private readonly ILogger<SignUpKeyService> _logger;

        public SignUpKeyService(
            ISignUpKeyRepository signUpKeyRepository,
            IValidator<CreateSignUpKeyRequest> createValidator,
            IValidator<UpdateSignUpKeyRequest> updateValidator,
            ILogger<SignUpKeyService> logger)
        {
            _signUpKeyRepository = signUpKeyRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        public async Task<SignUpKey?> GetByIdAsync(int id)
        {
            return await _signUpKeyRepository.GetByIdAsync(id);
        }

        public async Task<SignUpKey?> GetByKeyAsync(string key)
        {
            return await _signUpKeyRepository.GetByKeyAsync(key);
        }

        public async Task<IEnumerable<SignUpKey>> GetAllAsync()
        {
            return await _signUpKeyRepository.GetAllAsync();
        }

        public async Task<PagedResponse<SignUpKey>> GetPagedAsync(PaginationParameters parameters)
        {
            return await _signUpKeyRepository.GetPagedAsync(parameters);
        }

        public async Task<SignUpKey> CreateAsync(CreateSignUpKeyRequest request)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            string key;
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                key = Guid.NewGuid().ToString("N");

                while (await _signUpKeyRepository.KeyExistsAsync(key))
                {
                    key = Guid.NewGuid().ToString("N");
                }
            }
            else
            {
                key = request.Key.Trim();

                if (await _signUpKeyRepository.KeyExistsAsync(key))
                {
                    _logger.LogWarning("Create sign-up key rejected: key already exists");
                    throw new InvalidOperationException($"A sign-up key with value '{key}' already exists.");
                }
            }

            var signUpKey = SignUpKeyMapper.ToNewEntity(key, request.ExpiresAt);

            return await _signUpKeyRepository.AddAsync(signUpKey);
        }

        public async Task<SignUpKey> UpdateAsync(int id, UpdateSignUpKeyRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingSignUpKey = await _signUpKeyRepository.GetByIdAsync(id);
            if (existingSignUpKey == null)
            {
                _logger.LogWarning("Update sign-up key failed: id {SignUpKeyId} not found", id);
                throw new ArgumentException($"Sign-up key with ID {id} not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.Key))
            {
                var newKey = request.Key.Trim();

                var existingKey = await _signUpKeyRepository.GetByKeyAsync(newKey);
                if (existingKey != null && existingKey.Id != id)
                {
                    _logger.LogWarning("Update sign-up key rejected: duplicate key for id {SignUpKeyId}", id);
                    throw new InvalidOperationException($"A sign-up key with value '{newKey}' already exists.");
                }

                existingSignUpKey.Key = newKey;
            }

            if (request.ExpiresAt.HasValue)
            {
                existingSignUpKey.ExpiresAt = request.ExpiresAt.Value;
            }

            existingSignUpKey.UpdatedAt = DateTime.UtcNow;

            await _signUpKeyRepository.UpdateAsync(existingSignUpKey);
            return existingSignUpKey;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var signUpKey = await _signUpKeyRepository.GetByIdAsync(id);
            if (signUpKey == null)
            {
                return false;
            }

            await _signUpKeyRepository.DeleteAsync(signUpKey);
            return true;
        }

        public async Task<int> DeleteAllAsync()
        {
            var countBefore = await _signUpKeyRepository.CountAsync();
            await _signUpKeyRepository.DeleteAllAsync();
            return countBefore;
        }

        public async Task<bool> IsKeyValidAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var signUpKey = await _signUpKeyRepository.GetByKeyAsync(key);
            if (signUpKey == null)
            {
                return false;
            }

            return signUpKey.ExpiresAt >= DateTime.UtcNow;
        }

        public async Task<IEnumerable<SignUpKey>> GetActiveKeysAsync()
        {
            return await _signUpKeyRepository.GetActiveKeysAsync();
        }

        public async Task<IEnumerable<SignUpKey>> GetExpiredKeysAsync()
        {
            return await _signUpKeyRepository.GetExpiredKeysAsync();
        }
    }
}

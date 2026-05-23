using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.BusinessLogic.Interfaces.UserPermissions;
using Server.Mapping.UserPermissions;
using Server.Models;
using Server.Models.Logging;
using Server.Repositories.Interfaces;
using Shared.Dtos;
using Shared.Dtos.UserPermissions;

namespace Server.Controllers.UserPermissions
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Permission.AllPermissions")]
    public class SignUpKeyController : BaseController
    {
        private readonly ISignUpKeyService _signUpKeyService;
        private readonly ILoggingRepository _loggingRepository;
        private readonly ILogger<SignUpKeyController> _logger;

        public SignUpKeyController(
            ISignUpKeyService signUpKeyService,
            ILoggingRepository loggingRepository,
            ILogger<SignUpKeyController> logger)
        {
            _signUpKeyService = signUpKeyService;
            _loggingRepository = loggingRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all sign-up keys
        /// </summary>
        /// <returns>List of all sign-up keys</returns>
        [HttpGet(Name = "GetAllSignUpKeys")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SignUpKeyResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAllSignUpKeys()
        {
            var signUpKeys = await _signUpKeyService.GetAllAsync();
            var response = signUpKeys.Select(SignUpKeyMapper.ToResponse);
            return SuccessResponse(response, "Sign-up keys retrieved successfully");
        }

        /// <summary>
        /// Get a sign-up key by ID
        /// </summary>
        /// <param name="id">Sign-up key ID</param>
        /// <returns>Sign-up key details</returns>
        [HttpGet("{id:int}", Name = "GetSignUpKeyById")]
        [ProducesResponseType(typeof(ApiResponse<SignUpKeyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetSignUpKeyById(int id)
        {
            var signUpKey = await _signUpKeyService.GetByIdAsync(id);
            if (signUpKey == null)
            {
                return NotFoundResponse($"Sign-up key with ID {id} not found");
            }

            return SuccessResponse(SignUpKeyMapper.ToResponse(signUpKey), "Sign-up key retrieved successfully");
        }

        /// <summary>
        /// Get a sign-up key by key value
        /// </summary>
        /// <param name="key">Sign-up key value</param>
        /// <returns>Sign-up key details</returns>
        [HttpGet("key/{key}", Name = "GetSignUpKeyByKey")]
        [ProducesResponseType(typeof(ApiResponse<SignUpKeyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetSignUpKeyByKey(string key)
        {
            var signUpKey = await _signUpKeyService.GetByKeyAsync(key);
            if (signUpKey == null)
            {
                return NotFoundResponse($"Sign-up key with value '{key}' not found");
            }

            return SuccessResponse(SignUpKeyMapper.ToResponse(signUpKey), "Sign-up key retrieved successfully");
        }

        /// <summary>
        /// Get paginated list of sign-up keys
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of sign-up keys</returns>
        [HttpGet("paged", Name = "GetSignUpKeysPaged")]
        [ProducesResponseType(typeof(PaginatedApiResponse<SignUpKeyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetSignUpKeysPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var pagedResult = await _signUpKeyService.GetPagedAsync(parameters);
            var responseData = pagedResult.Items.Select(SignUpKeyMapper.ToResponse);

            return PaginatedResponse(
                responseData,
                pagedResult.PageNumber,
                pagedResult.PageSize,
                pagedResult.TotalCount,
                "Sign-up keys retrieved successfully");
        }

        /// <summary>
        /// Get all active (non-expired) sign-up keys
        /// </summary>
        /// <returns>List of active sign-up keys</returns>
        [HttpGet("active", Name = "GetActiveSignUpKeys")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SignUpKeyResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetActiveSignUpKeys()
        {
            var signUpKeys = await _signUpKeyService.GetActiveKeysAsync();
            var response = signUpKeys.Select(SignUpKeyMapper.ToResponse);
            return SuccessResponse(response, "Active sign-up keys retrieved successfully");
        }

        /// <summary>
        /// Get all expired sign-up keys
        /// </summary>
        /// <returns>List of expired sign-up keys</returns>
        [HttpGet("expired", Name = "GetExpiredSignUpKeys")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SignUpKeyResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetExpiredSignUpKeys()
        {
            var signUpKeys = await _signUpKeyService.GetExpiredKeysAsync();
            var response = signUpKeys.Select(SignUpKeyMapper.ToResponse);
            return SuccessResponse(response, "Expired sign-up keys retrieved successfully");
        }

        /// <summary>
        /// Check if a sign-up key is valid (exists and not expired)
        /// </summary>
        /// <param name="key">Sign-up key value</param>
        /// <returns>Validation result</returns>
        [HttpGet("validate/{key}", Name = "ValidateSignUpKey")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ValidateSignUpKey(string key)
        {
            var isValid = await _signUpKeyService.IsKeyValidAsync(key);
            return SuccessResponse(isValid, isValid ? "Sign-up key is valid" : "Sign-up key is invalid or expired");
        }

        /// <summary>
        /// Create a new sign-up key
        /// </summary>
        /// <param name="request">Sign-up key creation request</param>
        /// <returns>Created sign-up key</returns>
        [HttpPost(Name = "CreateSignUpKey")]
        [ProducesResponseType(typeof(ApiResponse<SignUpKeyResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateSignUpKey([FromBody] CreateSignUpKeyRequest request)
        {
            try
            {
                var signUpKey = await _signUpKeyService.CreateAsync(request);
                var response = SignUpKeyMapper.ToResponse(signUpKey);
                var location = Url.Action(nameof(GetSignUpKeyById), new { id = signUpKey.Id }) ?? string.Empty;
                return CreatedResponse(response, location, "Sign-up key created successfully");
            }
            catch (ValidationException ex)
            {
                var validationErrors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, new ApiError(ex.Message, "DUPLICATE_KEY", TraceId));
            }
        }

        /// <summary>
        /// Update an existing sign-up key
        /// </summary>
        /// <param name="id">Sign-up key ID</param>
        /// <param name="request">Sign-up key update request</param>
        /// <returns>Updated sign-up key</returns>
        [HttpPut("{id:int}", Name = "UpdateSignUpKey")]
        [ProducesResponseType(typeof(ApiResponse<SignUpKeyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateSignUpKey(int id, [FromBody] UpdateSignUpKeyRequest request)
        {
            try
            {
                var signUpKey = await _signUpKeyService.UpdateAsync(id, request);
                return SuccessResponse(SignUpKeyMapper.ToResponse(signUpKey), "Sign-up key updated successfully");
            }
            catch (ValidationException ex)
            {
                var validationErrors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                return ValidationErrorResponse(validationErrors);
            }
            catch (ArgumentException ex)
            {
                // Could be "not found" or "duplicate" - check message
                if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFoundResponse(ex.Message);
                }
                return BadRequestResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, new ApiError(ex.Message, "DUPLICATE_KEY", TraceId));
            }
        }

        /// <summary>
        /// Delete a sign-up key
        /// </summary>
        /// <param name="id">Sign-up key ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:int}", Name = "DeleteSignUpKey")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteSignUpKey(int id)
        {
            var deleted = await _signUpKeyService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFoundResponse($"Sign-up key with ID {id} not found");
            }

            return SuccessResponse(deleted, "Sign-up key deleted successfully");
        }

        /// <summary>
        /// Delete all sign-up keys
        /// </summary>
        /// <returns>Number of keys deleted</returns>
        [HttpDelete("all", Name = "DeleteAllSignUpKeys")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteAllSignUpKeys()
        {
            try
            {
                var deletedCount = await _signUpKeyService.DeleteAllAsync();
                return SuccessResponse(deletedCount, $"All sign-up keys deleted successfully. {deletedCount} keys were removed.");
            }
            catch (Exception ex)
            {
                try
                {
                    await _loggingRepository.AddAsync(new ErrorLogging
                    {
                        Message = $"[{TraceId}] {ex.Message}",
                        StackTrace = ex.ToString(),
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = nameof(SignUpKeyController)
                    });
                }
                catch (Exception)
                {
                    _logger.LogCritical(
                        "Failed to persist error to ErrorLogs while deleting all sign-up keys. TraceId: {TraceId}",
                        TraceId);
                }

                _logger.LogError(
                    "Error deleting all sign-up keys. TraceId: {TraceId}. Full details were written to ErrorLogs.",
                    TraceId);

                return InternalServerErrorResponse("An unexpected error occurred while deleting sign-up keys.");
            }
        }
    }
}

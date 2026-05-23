using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Controllers;
using Server.Models.AppSettings;
using Server.Repositories.Interfaces.AppSettings;
using Shared.Dtos;
using Shared.Dtos.Users;

namespace Server.Controllers.AppSettings
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserSettingsController : BaseController
    {
        private readonly IUserSettingsRepository _userSettingsRepository;

        public UserSettingsController(IUserSettingsRepository userSettingsRepository)
        {
            _userSettingsRepository = userSettingsRepository;
        }

        /// <summary>
        /// Get settings for the authenticated user (creates defaults if missing).
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<UserSettingsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetMine()
        {
            if (!TryGetCurrentUserId(out var userId, out var unauthorized))
                return unauthorized;

            var username = User.Identity?.Name ?? string.Empty;
            var settings = await GetOrCreateSettingsAsync(userId, username, darkMode: true);
            return SuccessResponse(ToResponse(settings), "User settings retrieved successfully");
        }

        /// <summary>
        /// Update settings for the authenticated user (creates a row if missing).
        /// </summary>
        [HttpPut("me")]
        [ProducesResponseType(typeof(ApiResponse<UserSettingsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> UpdateMine([FromBody] UpdateUserSettingsRequest request)
        {
            if (!TryGetCurrentUserId(out var userId, out var unauthorized))
                return unauthorized;

            var username = User.Identity?.Name ?? string.Empty;
            var settings = await _userSettingsRepository.GetByUserIdAsync(userId);
            if (settings == null)
            {
                var now = DateTimeOffset.UtcNow;
                settings = await _userSettingsRepository.AddAsync(new UserSettings
                {
                    UserId = userId,
                    DarkMode = request.DarkMode,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = username,
                    UpdatedBy = username
                });
                return SuccessResponse(ToResponse(settings), "User settings updated successfully");
            }

            settings.DarkMode = request.DarkMode;
            settings.UpdatedAt = DateTimeOffset.UtcNow;
            settings.UpdatedBy = username;

            await _userSettingsRepository.UpdateAsync(settings);

            return SuccessResponse(ToResponse(settings), "User settings updated successfully");
        }

        private async Task<UserSettings> GetOrCreateSettingsAsync(int userId, string username, bool darkMode)
        {
            var existing = await _userSettingsRepository.GetByUserIdAsync(userId);
            if (existing != null)
                return existing;

            var now = DateTimeOffset.UtcNow;
            return await _userSettingsRepository.AddAsync(new UserSettings
            {
                UserId = userId,
                DarkMode = darkMode,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = username,
                UpdatedBy = username
            });
        }

        private bool TryGetCurrentUserId(out int userId, out ActionResult unauthorized)
        {
            unauthorized = null!;
            userId = 0;
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out userId))
            {
                unauthorized = StatusCode(401, new ApiError("User identity not found", "UNAUTHORIZED", TraceId));
                return false;
            }

            return true;
        }

        private static UserSettingsResponse ToResponse(UserSettings entity) => new()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            DarkMode = entity.DarkMode
        };
    }
}

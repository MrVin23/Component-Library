using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Database;
using Server.Utils.UserPermissions;
using Shared.Dtos;

namespace Server.Controllers.UserPermissions
{
    [ApiController]
    [Route("api/test-auth")]
    public class TestAuthController : BaseController
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly DatabaseContext _dbContext;

        public TestAuthController(
            IAuthorizationService authorizationService,
            DatabaseContext dbContext)
        {
            _authorizationService = authorizationService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Test if current user has a specific permission from database mapping.
        /// </summary>
        /// <param name="permissionName">Name of the permission to check</param>
        /// <returns>True if user has access, false otherwise</returns>
        [HttpGet("permission/{permissionName}")]
        [Authorize] // Requires authentication
        [ProducesResponseType(typeof(ApiResponse<PermissionTestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> TestPermission(string permissionName)
        {
            var requirement = new PermissionRequirement(permissionName);
            var result = await _authorizationService.AuthorizeAsync(User, null, requirement);

            return SuccessResponse(new PermissionTestResponse
            {
                HasAccess = result.Succeeded,
                Permission = permissionName,
                Message = result.Succeeded 
                    ? $"User has access to permission: {permissionName}" 
                    : $"User does not have access to permission: {permissionName}",
                UserId = User.GetUserId(),
                Username = User.GetUsername() ?? "Unknown"
            }, "Permission test completed");
        }

        /// <summary>
        /// Access allowed only if user has read-only permission.
        /// </summary>
        [HttpGet("can-access/read-only")]
        [Authorize(Policy = "Permission.ReadOnly")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
        public ActionResult CanAccessReadOnly()
        {
            return SuccessResponse(new
            {
                HasAccess = true,
                Permission = PermissionNames.ReadOnly,
                Message = "Access granted for read-only permission.",
                UserId = User.GetUserId(),
                Username = User.GetUsername() ?? "Unknown"
            }, "Permission-protected endpoint passed");
        }

        /// <summary>
        /// Access allowed only if user has read-write permission.
        /// </summary>
        [HttpGet("can-access/read-write")]
        [Authorize(Policy = "Permission.ReadWrite")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
        public ActionResult CanAccessReadWrite()
        {
            return SuccessResponse(new
            {
                HasAccess = true,
                Permission = PermissionNames.ReadWrite,
                Message = "Access granted for read-write permission.",
                UserId = User.GetUserId(),
                Username = User.GetUsername() ?? "Unknown"
            }, "Permission-protected endpoint passed");
        }

        /// <summary>
        /// Access allowed only if user has all-permissions permission.
        /// </summary>
        [HttpGet("can-access/all-permissions")]
        [Authorize(Policy = "Permission.AllPermissions")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
        public ActionResult CanAccessAllPermissions()
        {
            return SuccessResponse(new
            {
                HasAccess = true,
                Permission = PermissionNames.AllPermissions,
                Message = "Access granted for all-permissions.",
                UserId = User.GetUserId(),
                Username = User.GetUsername() ?? "Unknown"
            }, "Permission-protected endpoint passed");
        }

        /// <summary>
        /// Returns effective permissions resolved from the database for the current user.
        /// </summary>
        /// <returns>List of all permissions the user has through assigned roles.</returns>
        [HttpGet("my-permissions")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserPermissionsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetMyPermissions()
        {
            var userId = User.GetUserId();
            if (userId == 0)
            {
                return StatusCode(401, new ApiError("User ID not found", "UNAUTHORIZED", TraceId));
            }

            var permissions = await _dbContext.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.UserRoles)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .OrderBy(static p => p)
                .ToListAsync();

            var response = new UserPermissionsResponse
            {
                UserId = userId,
                Username = User.GetUsername() ?? "Unknown",
                Permissions = permissions
            };

            return SuccessResponse(response, "User permissions retrieved from database");
        }
    }

    public class PermissionTestResponse
    {
        public bool HasAccess { get; set; }
        public string Permission { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class UserPermissionsResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public IReadOnlyList<string> Permissions { get; set; } = [];
    }
}

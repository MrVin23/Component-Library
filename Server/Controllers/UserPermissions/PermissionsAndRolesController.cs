using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.BusinessLogic.Interfaces.UserPermissions;
using Server.Mapping.UserPermissions;
using Server.Models;
using Server.Models.UserPermissions;
using Server.Repositories.Interfaces.UserPermissions;
using Shared.Dtos;
using Shared.Dtos.UserPermissions;

namespace Server.Controllers.UserPermissions
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Permission.AllPermissions")]
    public class PermissionsAndRolesController : BaseController
    {
        private readonly IRolesAndPermissionsService _rolesAndPermissionsService;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public PermissionsAndRolesController(
            IRolesAndPermissionsService rolesAndPermissionsService,
            IPermissionRepository permissionRepository,
            IUserRoleRepository userRoleRepository)
        {
            _rolesAndPermissionsService = rolesAndPermissionsService;
            _permissionRepository = permissionRepository;
            _userRoleRepository = userRoleRepository;
        }

        /// <summary>
        /// Get a paginated list of all role permissions
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of role permissions</returns>
        [HttpGet("role-permissions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RolePermissionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetRolePermissions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _rolesAndPermissionsService.GetRolePermissionsPagedAsync(parameters);
            
            var rolePermissionResponses = result.Items.Cast<RolePermission>()
                .Select(RolesAndPermissionsMapper.ToRolePermissionResponse);

            return PaginatedResponse(
                rolePermissionResponses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Role permissions retrieved successfully"
            );
        }

        /// <summary>
        /// Get permissions for a specific role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of permissions for the role</returns>
        [HttpGet("roles/{roleId}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetPermissionsByRole(int roleId)
        {
            try
            {
                var permissions = await _rolesAndPermissionsService.GetPermissionsByRoleAsync(roleId);
                var role = await _rolesAndPermissionsService.GetRoleByIdAsync(roleId);
                
                var permissionResponses = permissions.Select(RolesAndPermissionsMapper.ToPermissionResponse);

                return SuccessResponse(permissionResponses, $"Permissions for role '{role?.Name}' retrieved successfully");
            }
            catch (ArgumentException ex)
            {
                return NotFoundResponse(ex.Message);
            }
        }

        /// <summary>
        /// Set permissions to a role (replaces existing permissions)
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="request">Permission IDs to set</param>
        /// <returns>Updated role permissions</returns>
        [HttpPut("roles/{roleId}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SetRolePermissions(int roleId, [FromBody] SetPermissionsRequest request)
        {
            try
            {
                await _rolesAndPermissionsService.SetRolePermissionsAsync(roleId, request.PermissionIds);

                // Retrieve updated permissions
                var updatedPermissions = await _rolesAndPermissionsService.GetPermissionsByRoleAsync(roleId);
                var role = await _rolesAndPermissionsService.GetRoleByIdAsync(roleId);
                
                var permissionResponses = updatedPermissions.Select(RolesAndPermissionsMapper.ToPermissionResponse);

                return SuccessResponse(permissionResponses, $"Permissions updated for role '{role?.Name}'");
            }
            catch (ArgumentException ex)
            {
                return NotFoundResponse(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequestResponse(string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
            }
        }

        /// <summary>
        /// Remove a permission from a role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemovePermissionFromRole(int roleId, int permissionId)
        {
            try
            {
                await _rolesAndPermissionsService.RemovePermissionFromRoleAsync(roleId, permissionId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFoundResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequestResponse(ex.Message);
            }
        }

        /// <summary>
        /// Get a paginated list of all roles with their associated permissions
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of roles with permissions</returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleWithPermissionsResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetRoles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _rolesAndPermissionsService.GetRolesPagedAsync(parameters);
            
            var rolesWithPermissions = new List<RoleWithPermissionsResponse>();
            foreach (var role in result.Items.Cast<Role>())
            {
                var permissions = await _rolesAndPermissionsService.GetPermissionsByRoleAsync(role.Id);
                rolesWithPermissions.Add(RolesAndPermissionsMapper.ToRoleWithPermissionsResponse(role, permissions));
            }

            return PaginatedResponse(
                rolesWithPermissions,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Roles retrieved successfully"
            );
        }

        /// <summary>
        /// Get users with their roles and permissions
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of users with roles and permissions</returns>
        [HttpGet("users")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserWithRolesResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetUsersWithRoles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Use queryable with includes to load navigation properties
            var allUserRoles = await _userRoleRepository.GetQueryable()
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .ToListAsync();
            
            var userRoles = allUserRoles.Cast<UserRole>().ToList();

            // Group by user
            var groupedUserRoles = userRoles.GroupBy(ur => ur.UserId).ToList();
            
            var usersWithRoles = new List<UserWithRolesResponse>();
            
            foreach (var group in groupedUserRoles.Skip((pageNumber - 1) * pageSize).Take(pageSize))
            {
                var userRolesForUser = group.ToList();
                var firstUserRole = userRolesForUser.First();
                
                var rolesWithPermissions = new List<RoleWithPermissionsResponse>();
                
                foreach (var ur in userRolesForUser)
                {
                    if (ur.Role == null) continue;
                    
                    // Fetch permissions for this role
                    var permissions = await _permissionRepository.GetPermissionsByRoleAsync(ur.Role.Id);

                    rolesWithPermissions.Add(RolesAndPermissionsMapper.ToRoleWithPermissionsResponse(ur.Role, permissions));
                }

                usersWithRoles.Add(RolesAndPermissionsMapper.ToUserWithRolesResponse(firstUserRole, rolesWithPermissions));
            }

            var totalCount = groupedUserRoles.Count;

            return PaginatedResponse(
                usersWithRoles,
                pageNumber,
                pageSize,
                totalCount,
                "Users with roles retrieved successfully"
            );
        }

        /// <summary>
        /// Get a paginated list of all user-role assignments (flat table)
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of user-role assignments</returns>
        [HttpGet("user-roles")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserRoleResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetUserRoles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var allUserRoles = await _userRoleRepository.GetQueryable()
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();

            var userRoles = allUserRoles.Cast<UserRole>().ToList();
            var totalCount = userRoles.Count;

            var pagedUserRoles = userRoles
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(RolesAndPermissionsMapper.ToUserRoleResponse);

            return PaginatedResponse(
                pagedUserRoles,
                pageNumber,
                pageSize,
                totalCount,
                "User-role assignments retrieved successfully"
            );
        }

        /// <summary>
        /// Remove a role from a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("users/{userId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemoveUserRole(int userId, int roleId)
        {
            var userRole = await _userRoleRepository.GetUserRoleAsync(userId, roleId);
            if (userRole == null)
            {
                return NotFoundResponse($"User with ID {userId} does not have role with ID {roleId}");
            }

            await _userRoleRepository.RemoveUserRoleAsync(userId, roleId);
            return NoContent();
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>Success result</returns>
        [HttpPost("users/{userId}/roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AssignRoleToUser(int userId, int roleId)
        {
            var role = await _rolesAndPermissionsService.GetRoleByIdAsync(roleId);
            if (role == null)
            {
                return NotFoundResponse($"Role with ID {roleId} not found");
            }

            // Check if user exists by getting repositories from the same context
            // For now, we'll check if the relationship already exists
            var hasRole = await _userRoleRepository.UserHasRoleAsync(userId, roleId);
            if (hasRole)
            {
                return BadRequestResponse($"User with ID {userId} already has role '{role.Name}'");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };

            var addedUserRole = await _userRoleRepository.AddAsync(userRole);
            
            var response = new
            {
                Id = addedUserRole.Id,
                UserId = addedUserRole.UserId,
                RoleId = addedUserRole.RoleId,
                RoleName = role.Name,
                CreatedAt = addedUserRole.CreatedAt,
                UpdatedAt = addedUserRole.UpdatedAt
            };

            return StatusCode(201, new ApiResponse<object>(response, $"Role '{role.Name}' assigned to user successfully")
            {
                TraceId = TraceId
            });
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="request">Role creation request</param>
        /// <returns>Created role</returns>
        [HttpPost("roles")]
        [ProducesResponseType(typeof(ApiResponse<RoleWithPermissionsResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            try
            {
                var createdRole = await _rolesAndPermissionsService.CreateRoleAsync(request);

                var response = RolesAndPermissionsMapper.ToRoleWithPermissionsResponse(createdRole);

                var location = Url.Action(nameof(GetPermissionsByRole), new { roleId = createdRole.Id }) ?? string.Empty;
                return CreatedResponse(response, location, "Role created successfully");
            }
            catch (ValidationException ex)
            {
                return BadRequestResponse(string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, new ApiError(ex.Message, "DUPLICATE_ROLE", TraceId));
            }
        }

        /// <summary>
        /// Update an existing role
        /// </summary>
        [HttpPut("roles/{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<RoleWithPermissionsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> UpdateRole(int roleId, [FromBody] UpdateRoleRequest request)
        {
            try
            {
                var updatedRole = await _rolesAndPermissionsService.UpdateRoleAsync(roleId, request);
                var permissions = await _rolesAndPermissionsService.GetPermissionsByRoleAsync(roleId);
                var response = RolesAndPermissionsMapper.ToRoleWithPermissionsResponse(updatedRole, permissions);
                return SuccessResponse(response, "Role updated successfully");
            }
            catch (ValidationException ex)
            {
                return BadRequestResponse(string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
            }
            catch (ArgumentException ex)
            {
                return NotFoundResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, new ApiError(ex.Message, "DUPLICATE_ROLE", TraceId));
            }
        }

        /// <summary>
        /// Delete a role by ID
        /// </summary>
        [HttpDelete("roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteRole(int roleId)
        {
            var deleted = await _rolesAndPermissionsService.DeleteRoleAsync(roleId);
            if (!deleted)
            {
                return NotFoundResponse($"Role with ID {roleId} not found.");
            }

            return NoContent();
        }

        /// <summary>
        /// Get a paginated list of all permissions
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of permissions</returns>
        [HttpGet("permissions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetPermissions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _rolesAndPermissionsService.GetPermissionsPagedAsync(parameters);

            var permissionResponses = result.Items.Cast<Permission>()
                .Select(RolesAndPermissionsMapper.ToPermissionResponse);

            return PaginatedResponse(
                permissionResponses,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Permissions retrieved successfully"
            );
        }

        /// <summary>
        /// Create a new permission
        /// </summary>
        /// <param name="request">Permission creation request</param>
        /// <returns>Created permission</returns>
        [HttpPost("permissions")]
        [ProducesResponseType(typeof(ApiResponse<PermissionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
        {
            try
            {
                var createdPermission = await _rolesAndPermissionsService.CreatePermissionAsync(request);

                var response = RolesAndPermissionsMapper.ToPermissionResponse(createdPermission);

                return StatusCode(201, new ApiResponse<PermissionResponse>(response, "Permission created successfully")
                {
                    TraceId = TraceId
                });
            }
            catch (ValidationException ex)
            {
                return BadRequestResponse(string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, new ApiError(ex.Message, "DUPLICATE_PERMISSION", TraceId));
            }
        }

        /// <summary>
        /// Update an existing permission
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        /// <param name="request">Permission update request</param>
        /// <returns>Updated permission</returns>
        [HttpPut("permissions/{permissionId}")]
        [ProducesResponseType(typeof(ApiResponse<PermissionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> UpdatePermission(int permissionId, [FromBody] UpdatePermissionRequest request)
        {
            try
            {
                var updatedPermission = await _rolesAndPermissionsService.UpdatePermissionAsync(permissionId, request);
                var response = RolesAndPermissionsMapper.ToPermissionResponse(updatedPermission);
                return SuccessResponse(response, "Permission updated successfully");
            }
            catch (ValidationException ex)
            {
                return BadRequestResponse(string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
            }
            catch (ArgumentException ex)
            {
                return NotFoundResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, new ApiError(ex.Message, "DUPLICATE_PERMISSION", TraceId));
            }
        }

        /// <summary>
        /// Delete a permission by ID
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("permissions/{permissionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePermission(int permissionId)
        {
            var deleted = await _rolesAndPermissionsService.DeletePermissionAsync(permissionId);
            if (!deleted)
            {
                return NotFoundResponse($"Permission with ID {permissionId} not found.");
            }

            return NoContent();
        }

        /// <summary>
        /// Assign a permission to a role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>Created role permission relationship</returns>
        [HttpPost("roles/{roleId}/permissions/{permissionId}")]
        [ProducesResponseType(typeof(ApiResponse<RolePermissionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<ActionResult> AssignPermissionToRole(int roleId, int permissionId)
        {
            try
            {
                await _rolesAndPermissionsService.AssignPermissionToRoleAsync(roleId, permissionId);

                var role = await _rolesAndPermissionsService.GetRoleByIdAsync(roleId);
                var permission = await _rolesAndPermissionsService.GetPermissionByIdAsync(permissionId);

                var response = RolesAndPermissionsMapper.ToRolePermissionAssignmentResponse(
                    roleId,
                    role?.Name,
                    permissionId,
                    permission?.Name);

                return StatusCode(201, new ApiResponse<RolePermissionResponse>(response, $"Permission '{permission?.Name}' assigned to role '{role?.Name}' successfully")
                {
                    TraceId = TraceId
                });
            }
            catch (ArgumentException ex)
            {
                return NotFoundResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, new ApiError(ex.Message, "DUPLICATE_PERMISSION_ASSIGNMENT", TraceId));
            }
            catch (ValidationException ex)
            {
                return BadRequestResponse(string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
            }
        }
    }
}
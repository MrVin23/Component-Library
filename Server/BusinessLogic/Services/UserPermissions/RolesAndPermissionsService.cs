using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.BusinessLogic.Interfaces.UserPermissions;
using Server.Models;
using Server.Models.UserPermissions;
using Server.Repositories.Interfaces.UserPermissions;
using Server.Mapping.UserPermissions;
using Shared.Dtos.UserPermissions;

namespace Server.BusinessLogic.Services.UserPermissions
{
    public class RolesAndPermissionsService : IRolesAndPermissionsService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IValidator<CreateRoleRequest> _createRoleValidator;
        private readonly IValidator<UpdateRoleRequest> _updateRoleValidator;
        private readonly IValidator<CreatePermissionRequest> _createPermissionValidator;
        private readonly IValidator<UpdatePermissionRequest> _updatePermissionValidator;
        private readonly IValidator<SetPermissionsRequest> _setPermissionsValidator;
        private readonly IValidator<AssignPermissionToRoleRequest> _assignPermissionToRoleValidator;
        private readonly ILogger<RolesAndPermissionsService> _logger;

        public RolesAndPermissionsService(
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IRolePermissionRepository rolePermissionRepository,
            IValidator<CreateRoleRequest> createRoleValidator,
            IValidator<UpdateRoleRequest> updateRoleValidator,
            IValidator<CreatePermissionRequest> createPermissionValidator,
            IValidator<UpdatePermissionRequest> updatePermissionValidator,
            IValidator<SetPermissionsRequest> setPermissionsValidator,
            IValidator<AssignPermissionToRoleRequest> assignPermissionToRoleValidator,
            ILogger<RolesAndPermissionsService> logger)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _createRoleValidator = createRoleValidator;
            _updateRoleValidator = updateRoleValidator;
            _createPermissionValidator = createPermissionValidator;
            _updatePermissionValidator = updatePermissionValidator;
            _setPermissionsValidator = setPermissionsValidator;
            _assignPermissionToRoleValidator = assignPermissionToRoleValidator;
            _logger = logger;
        }

        #region Role Operations

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _roleRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<PagedResponse<Role>> GetRolesPagedAsync(PaginationParameters parameters)
        {
            return await _roleRepository.GetPagedAsync(parameters);
        }

        public async Task<Role> CreateRoleAsync(CreateRoleRequest request)
        {
            request.Name = request.Name?.Trim() ?? string.Empty;

            var validationResult = await _createRoleValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var roleExists = await _roleRepository.RoleNameExistsAsync(request.Name);
            if (roleExists)
            {
                _logger.LogWarning("Create role rejected: name already exists {RoleName}", request.Name);
                throw new InvalidOperationException($"A role with name '{request.Name}' already exists.");
            }

            var role = RolesAndPermissionsMapper.ToNewRole(request);

            return await _roleRepository.AddAsync(role);
        }

        public async Task<Role> UpdateRoleAsync(int id, UpdateRoleRequest request)
        {
            request.Name = request.Name?.Trim() ?? string.Empty;

            var validationResult = await _updateRoleValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingRole = await _roleRepository.GetByIdAsync(id);
            if (existingRole == null)
            {
                _logger.LogWarning("Update role failed: role {RoleId} not found", id);
                throw new ArgumentException($"Role with ID {id} not found.");
            }

            var existingRoleWithName = await _roleRepository.GetByNameAsync(request.Name);
            if (existingRoleWithName != null && existingRoleWithName.Id != id)
            {
                _logger.LogWarning("Update role rejected: name already exists {RoleName}", request.Name);
                throw new InvalidOperationException($"A role with name '{request.Name}' already exists.");
            }

            existingRole.Name = request.Name;
            existingRole.Description = request.Description ?? string.Empty;
            existingRole.UpdatedAt = DateTime.UtcNow;

            await _roleRepository.UpdateAsync(existingRole);
            return existingRole;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return false;
            }

            await _roleRepository.DeleteAsync(role);
            return true;
        }

        #endregion

        #region Permission Operations

        public async Task<Permission?> GetPermissionByIdAsync(int id)
        {
            return await _permissionRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return await _permissionRepository.GetAllAsync();
        }

        public async Task<PagedResponse<Permission>> GetPermissionsPagedAsync(PaginationParameters parameters)
        {
            return await _permissionRepository.GetPagedAsync(parameters);
        }

        public async Task<Permission> CreatePermissionAsync(CreatePermissionRequest request)
        {
            request.Name = request.Name?.Trim() ?? string.Empty;

            var validationResult = await _createPermissionValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var permissionExists = await _permissionRepository.PermissionNameExistsAsync(request.Name);
            if (permissionExists)
            {
                _logger.LogWarning("Create permission rejected: name already exists {PermissionName}", request.Name);
                throw new InvalidOperationException($"A permission with name '{request.Name}' already exists.");
            }

            var permission = RolesAndPermissionsMapper.ToNewPermission(request);

            return await _permissionRepository.AddAsync(permission);
        }

        public async Task<Permission> UpdatePermissionAsync(int id, UpdatePermissionRequest request)
        {
            request.Name = request.Name?.Trim() ?? string.Empty;

            var validationResult = await _updatePermissionValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingPermission = await _permissionRepository.GetByIdAsync(id);
            if (existingPermission == null)
            {
                _logger.LogWarning("Update permission failed: permission {PermissionId} not found", id);
                throw new ArgumentException($"Permission with ID {id} not found.");
            }

            var existingPermissionWithName = await _permissionRepository.GetByNameAsync(request.Name);
            if (existingPermissionWithName != null && existingPermissionWithName.Id != id)
            {
                _logger.LogWarning("Update permission rejected: name already exists {PermissionName}", request.Name);
                throw new InvalidOperationException($"A permission with name '{request.Name}' already exists.");
            }

            existingPermission.Name = request.Name;
            existingPermission.Description = request.Description ?? string.Empty;
            existingPermission.UpdatedAt = DateTime.UtcNow;

            await _permissionRepository.UpdateAsync(existingPermission);
            return existingPermission;
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
            {
                return false;
            }

            await _permissionRepository.DeleteAsync(permission);
            return true;
        }

        #endregion

        #region Role-Permission Operations

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(int roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                _logger.LogWarning("Get permissions by role failed: role {RoleId} not found", roleId);
                throw new ArgumentException($"Role with ID {roleId} not found.");
            }

            return await _permissionRepository.GetPermissionsByRoleAsync(roleId);
        }

        public async Task AssignPermissionToRoleAsync(int roleId, int permissionId)
        {
            var assignRequest = new AssignPermissionToRoleRequest
            {
                RoleId = roleId,
                PermissionId = permissionId
            };
            var validationResult = await _assignPermissionToRoleValidator.ValidateAsync(assignRequest);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                _logger.LogWarning("Assign permission failed: role {RoleId} not found", roleId);
                throw new ArgumentException($"Role with ID {roleId} not found.");
            }

            var permission = await _permissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
            {
                _logger.LogWarning("Assign permission failed: permission {PermissionId} not found", permissionId);
                throw new ArgumentException($"Permission with ID {permissionId} not found.");
            }

            var exists = await _rolePermissionRepository.RoleHasPermissionAsync(roleId, permissionId);
            if (exists)
            {
                _logger.LogWarning(
                    "Assign permission rejected: role {RoleId} already has permission {PermissionId}",
                    roleId, permissionId);
                throw new InvalidOperationException($"Role '{role.Name}' already has permission '{permission.Name}'.");
            }

            var rolePermission = RolesAndPermissionsMapper.ToNewRolePermission(roleId, permissionId);

            await _rolePermissionRepository.AddAsync(rolePermission);
        }

        public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                _logger.LogWarning("Remove permission failed: role {RoleId} not found", roleId);
                throw new ArgumentException($"Role with ID {roleId} not found.");
            }

            var permission = await _permissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
            {
                _logger.LogWarning("Remove permission failed: permission {PermissionId} not found", permissionId);
                throw new ArgumentException($"Permission with ID {permissionId} not found.");
            }

            var hasPermission = await _rolePermissionRepository.RoleHasPermissionAsync(roleId, permissionId);
            if (!hasPermission)
            {
                _logger.LogWarning(
                    "Remove permission rejected: role {RoleId} does not have permission {PermissionId}",
                    roleId, permissionId);
                throw new InvalidOperationException($"Role '{role.Name}' does not have permission '{permission.Name}'.");
            }

            await _rolePermissionRepository.RemoveRolePermissionAsync(roleId, permissionId);
        }

        public async Task SetRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                _logger.LogWarning("Set role permissions failed: role {RoleId} not found", roleId);
                throw new ArgumentException($"Role with ID {roleId} not found.");
            }

            var permissionIdList = permissionIds.ToList();

            var setPermissionsRequest = new SetPermissionsRequest { PermissionIds = permissionIdList };
            var validationResult = await _setPermissionsValidator.ValidateAsync(setPermissionsRequest);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            foreach (var permissionId in permissionIdList)
            {
                var permission = await _permissionRepository.GetByIdAsync(permissionId);
                if (permission == null)
                {
                    _logger.LogWarning(
                        "Set role permissions failed: permission {PermissionId} not found for role {RoleId}",
                        permissionId, roleId);
                    throw new ArgumentException($"Permission with ID {permissionId} not found.");
                }
            }

            await _rolePermissionRepository.UpdateRolePermissionsAsync(roleId, permissionIdList);
        }

        public async Task<bool> RoleHasPermissionAsync(int roleId, int permissionId)
        {
            return await _rolePermissionRepository.RoleHasPermissionAsync(roleId, permissionId);
        }

        public async Task<PagedResponse<RolePermission>> GetRolePermissionsPagedAsync(PaginationParameters parameters)
        {
            var query = _rolePermissionRepository.GetQueryable()
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission);

            return await query.ToPagedResponseAsync(parameters);
        }

        #endregion
    }
}

using FluentValidation;
using Shared.Dtos.UserPermissions;

namespace Shared.Validators
{
    /// <summary>
    /// Validator for CreateRoleRequest
    /// </summary>
    public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Role name is required")
                .MaximumLength(100)
                .WithMessage("Role name must not exceed 100 characters")
                .Must(name => name == name?.Trim())
                .WithMessage("Role name cannot have leading or trailing spaces")
                .Matches(@"^[a-zA-Z0-9_\-\s]+$")
                .WithMessage("Role name can only contain letters, numbers, underscores, hyphens, and spaces");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters");
        }
    }

    /// <summary>
    /// Validator for UpdateRoleRequest
    /// </summary>
    public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
    {
        public UpdateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Role name is required")
                .MaximumLength(100)
                .WithMessage("Role name must not exceed 100 characters")
                .Must(name => name == name?.Trim())
                .WithMessage("Role name cannot have leading or trailing spaces")
                .Matches(@"^[a-zA-Z0-9_\-\s]+$")
                .WithMessage("Role name can only contain letters, numbers, underscores, hyphens, and spaces");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters");
        }
    }

    /// <summary>
    /// Validator for CreatePermissionRequest
    /// </summary>
    public class CreatePermissionRequestValidator : AbstractValidator<CreatePermissionRequest>
    {
        public CreatePermissionRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Permission name is required")
                .MaximumLength(100)
                .WithMessage("Permission name must not exceed 100 characters")
                .Must(name => name == name?.Trim())
                .WithMessage("Permission name cannot have leading or trailing spaces")
                .Matches(@"^[a-zA-Z0-9_\-\.:]+$")
                .WithMessage("Permission name can only contain letters, numbers, underscores, hyphens, dots, and colons");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters");
        }
    }

    /// <summary>
    /// Validator for UpdatePermissionRequest
    /// </summary>
    public class UpdatePermissionRequestValidator : AbstractValidator<UpdatePermissionRequest>
    {
        public UpdatePermissionRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Permission name is required")
                .MaximumLength(100)
                .WithMessage("Permission name must not exceed 100 characters")
                .Must(name => name == name?.Trim())
                .WithMessage("Permission name cannot have leading or trailing spaces")
                .Matches(@"^[a-zA-Z0-9_\-\.:]+$")
                .WithMessage("Permission name can only contain letters, numbers, underscores, hyphens, dots, and colons");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters");
        }
    }

    /// <summary>
    /// Validator for SetPermissionsRequest
    /// </summary>
    public class SetPermissionsRequestValidator : AbstractValidator<SetPermissionsRequest>
    {
        public SetPermissionsRequestValidator()
        {
            RuleFor(x => x.PermissionIds)
                .NotNull()
                .WithMessage("Permission IDs list cannot be null");

            RuleForEach(x => x.PermissionIds)
                .GreaterThan(0)
                .WithMessage("Permission ID must be a positive integer");

            // Check for duplicate permission IDs in the request
            RuleFor(x => x.PermissionIds)
                .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
                .WithMessage("Duplicate permission IDs are not allowed in the request");
        }
    }

    /// <summary>
    /// Validator for AssignPermissionToRoleRequest
    /// </summary>
    public class AssignPermissionToRoleRequestValidator : AbstractValidator<AssignPermissionToRoleRequest>
    {
        public AssignPermissionToRoleRequestValidator()
        {
            RuleFor(x => x.RoleId)
                .GreaterThan(0)
                .WithMessage("Role ID must be a positive integer");

            RuleFor(x => x.PermissionId)
                .GreaterThan(0)
                .WithMessage("Permission ID must be a positive integer");
        }
    }
}

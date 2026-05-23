using Microsoft.AspNetCore.Authorization;

namespace Server.Utils.UserPermissions
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string[] PermissionNames { get; }

        public PermissionRequirement(params string[] permissionNames)
        {
            if (permissionNames == null || permissionNames.Length == 0)
            {
                throw new ArgumentException("At least one permission name is required.", nameof(permissionNames));
            }

            PermissionNames = permissionNames
                .Where(static p => !string.IsNullOrWhiteSpace(p))
                .Select(static p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (PermissionNames.Length == 0)
            {
                throw new ArgumentException("At least one non-empty permission name is required.", nameof(permissionNames));
            }
        }
    }
}


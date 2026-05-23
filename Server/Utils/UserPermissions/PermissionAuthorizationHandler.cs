using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Server.Database;

namespace Server.Utils.UserPermissions
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
                return;

            var userId = context.User.GetUserId();
            if (userId == 0)
                return;

            // Use List<string> for reliable EF Core SQL IN(...) translation
            // (string[].Contains uses Npgsql's ANY() which can mis-parameterise)
            var permissionNames = requirement.PermissionNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var userPermissions = await dbContext.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.UserRoles)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();

            var hasPermission = userPermissions.Any(static p => !string.IsNullOrWhiteSpace(p))
                && userPermissions.Any(p => permissionNames.Contains(p.Trim()));

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}


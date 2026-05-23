using Microsoft.EntityFrameworkCore;
using Server.Models.Logging;
using Server.Models.UserPermissions;
using Server.Models.AppSettings;
using Server.Models.Users;

namespace Server.Database.Interfaces;

public interface IDatabaseContext : IDisposable, IAsyncDisposable
{
    DbSet<User> Users { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<SignUpKey> SignUpKeys { get; }
    DbSet<ErrorLogging> ErrorLogs { get; }
    DbSet<UserSettings> UserSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();

    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}

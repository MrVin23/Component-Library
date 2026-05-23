using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Server.Database;

/// <summary>
/// Used by EF Core tools (<c>dotnet ef</c>) so migrations do not need to build the full web host.
/// </summary>
public sealed class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var basePath = ResolveConfigurationDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new DatabaseContext(optionsBuilder.Options);
    }

    private static string ResolveConfigurationDirectory()
    {
        for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
        {
            var appsettings = Path.Combine(dir.FullName, "appsettings.json");
            if (File.Exists(appsettings))
                return dir.FullName;
        }

        throw new InvalidOperationException(
            "Could not find appsettings.json by walking up from the current directory. Run EF commands from the Server project directory.");
    }
}

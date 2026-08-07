using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence.Data;

/// <summary>
///     Shared config for Duende's <c>PersistedGrantDbContext</c> on the master DB. The
///     IdentityServer runtime, DbMigrator, and EF design-time must build the model identically.
/// </summary>
public static class DuendeOperationalStore
{
    /// <summary>Migrations live in this assembly (Migrations/Duende), not the Duende package.</summary>
    public const string MigrationsAssembly = "Logistics.Infrastructure.Persistence";

    public static void ConfigureDbContext(DbContextOptionsBuilder options, string? connectionString)
    {
        // No lazy-loading proxies: Duende's entities have no navigations
        options.UseNpgsql(connectionString ?? ConnectionStrings.LocalMaster, o =>
            {
                o.EnableRetryOnFailure(8, TimeSpan.FromSeconds(15), null);
                o.MigrationsAssembly(MigrationsAssembly);
            })
            .UseSnakeCaseNamingConvention();
    }
}

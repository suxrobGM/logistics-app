using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence.Data;

/// <summary>
///     Single source of truth for configuring Duende's <c>PersistedGrantDbContext</c> against the
///     master database. Used by the IdentityServer runtime, the DbMigrator, and EF design-time -
///     the model must be built identically in all three or migrations drift.
///     Persisting signing keys and grants here is what lets user sessions survive container
///     redeploys (the containers themselves are stateless, no volumes).
/// </summary>
public static class DuendeOperationalStore
{
    /// <summary>Migrations live in this assembly (Migrations/Duende), not the Duende package.</summary>
    public const string MigrationsAssembly = "Logistics.Infrastructure.Persistence";

    public static void ConfigureDbContext(DbContextOptionsBuilder options, string? connectionString)
    {
        // No UseLazyLoadingProxies: Duende's operational entities have no navigation properties,
        // and keeping proxies out of this model keeps it byte-identical at design time.
        options.UseNpgsql(connectionString ?? ConnectionStrings.LocalMaster, o =>
            {
                o.EnableRetryOnFailure(8, TimeSpan.FromSeconds(15), null);
                o.MigrationsAssembly(MigrationsAssembly);
            })
            .UseSnakeCaseNamingConvention();
    }
}

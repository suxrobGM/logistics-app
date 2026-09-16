using Duende.IdentityServer.EntityFramework.Options;
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

    /// <summary>
    ///     Own schema: this context shares the master DB with <c>MasterDbContext</c>, so its tables
    ///     and its <c>__EFMigrationsHistory</c> would otherwise sit in public alongside the domain
    ///     ones, leaving `ef migrations list` and rollbacks unable to tell the two sets apart.
    /// </summary>
    public const string Schema = "duende";

    /// <summary>
    ///     Duende names its tables through <see cref="OperationalStoreOptions"/> rather than by
    ///     convention, so UseSnakeCaseNamingConvention reaches their columns but not the table
    ///     names. Set them here or they land as PascalCase beside otherwise snake_case tables.
    /// </summary>
    public static OperationalStoreOptions ConfigureStoreOptions(OperationalStoreOptions options)
    {
        options.DefaultSchema = Schema;
        options.PersistedGrants.Name = "persisted_grants";
        options.DeviceFlowCodes.Name = "device_codes";
        options.Keys.Name = "keys";
        options.ServerSideSessions.Name = "server_side_sessions";
        options.PushedAuthorizationRequests.Name = "pushed_authorization_requests";
        return options;
    }

    public static void ConfigureDbContext(DbContextOptionsBuilder options, string? connectionString)
    {
        // No lazy-loading proxies: Duende's entities have no navigations
        options.UseNpgsql(connectionString ?? ConnectionStrings.LocalMaster, o =>
            {
                o.EnableRetryOnFailure(8, TimeSpan.FromSeconds(15), null);
                o.MigrationsAssembly(MigrationsAssembly);
                o.MigrationsHistoryTable("__EFMigrationsHistory", Schema);
            })
            .UseSnakeCaseNamingConvention();
    }
}

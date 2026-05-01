using Logistics.Application.Services;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Regions;
using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Reads <c>Tenants[]</c> from configuration and upserts each demo tenant in the master DB,
/// provisioning its tenant database if missing. Replaces the legacy single-tenant
/// <c>DefaultTenantSeeder</c>.
/// </summary>
internal sealed class DemoTenantsSeeder(
    ILogger<DemoTenantsSeeder> logger,
    IRegionProfileFactory regionProfileFactory) : SeederBase(logger)
{
    public override string Name => nameof(DemoTenantsSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 40;

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var tenantConfigs = context.Configuration.GetSection("Tenants").Get<List<DemoTenantConfig>>();
        if (tenantConfigs is null || tenantConfigs.Count == 0)
        {
            logger.LogWarning("No 'Tenants' configured. Skipping demo tenant seeding.");
            LogCompleted();
            return;
        }

        var repo = context.MasterUnitOfWork.Repository<Tenant>();
        var databaseProvider = context.ServiceProvider.GetRequiredService<ITenantDatabaseService>();

        // Backfill: rename legacy "default" → "us" if found, so devs with existing master DBs
        // don't end up with both. The "us" tenant config supersedes "default".
        if (tenantConfigs.Any(c => c.Name == "us"))
        {
            var legacy = await repo.GetAsync(t => t.Name == "default", cancellationToken);
            if (legacy is not null && await repo.GetAsync(t => t.Name == "us", cancellationToken) is null)
            {
                legacy.Name = "us";
                await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Renamed legacy tenant 'default' → 'us' in master DB");
            }
        }

        foreach (var config in tenantConfigs)
        {
            var profile = regionProfileFactory.Get(config.Region);
            var connectionString = ResolveConnectionString(config, context.Configuration)
                ?? throw new InvalidOperationException(
                    $"No connection string for tenant '{config.Name}'. Set Tenants[].ConnectionString " +
                    $"or ConnectionStrings:{config.Name}TenantDatabase.");
            var existing = await repo.GetAsync(t => t.Name == config.Name, cancellationToken);

            if (existing is null)
            {
                var tenant = new Tenant
                {
                    Name = config.Name,
                    CompanyName = config.CompanyName,
                    BillingEmail = config.BillingEmail,
                    CompanyAddress = profile.CompanyAddress,
                    ConnectionString = connectionString,
                    IsSubscriptionRequired = false
                };
                ApplyRegionalSettings(tenant, profile);

                await repo.AddAsync(tenant, cancellationToken);
                await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
                await databaseProvider.CreateDatabaseAsync(tenant.ConnectionString);
                logger.LogInformation("Created tenant '{Name}' ({Region})", tenant.Name, profile.Region);
            }
            else
            {
                var updated = false;
                if (existing.CompanyName != config.CompanyName) { existing.CompanyName = config.CompanyName; updated = true; }
                if (existing.BillingEmail != config.BillingEmail) { existing.BillingEmail = config.BillingEmail; updated = true; }
                if (existing.ConnectionString != connectionString) { existing.ConnectionString = connectionString; updated = true; }
                if (existing.Settings.Region != profile.Region)
                {
                    ApplyRegionalSettings(existing, profile);
                    updated = true;
                }

                if (updated)
                {
                    await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Updated tenant '{Name}'", existing.Name);
                }

                // Ensure DB exists even for existing tenants (idempotent — no-op if present).
                await databaseProvider.CreateDatabaseAsync(existing.ConnectionString);
            }
        }

        LogCompleted(tenantConfigs.Count);
    }

    private static string? ResolveConnectionString(DemoTenantConfig config, IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(config.ConnectionString))
        {
            return config.ConnectionString;
        }
        // Fallback: ConnectionStrings:{Name}TenantDatabase — populated by Aspire's WithReference()
        // (e.g. "us" → "ConnectionStrings:UsTenantDatabase").
        var key = char.ToUpperInvariant(config.Name[0]) + config.Name[1..] + "TenantDatabase";
        return configuration.GetConnectionString(key);
    }

    private static void ApplyRegionalSettings(Tenant tenant, IRegionProfile profile)
    {
        tenant.Settings.Region = profile.Region;
        tenant.Settings.Currency = profile.Currency;
        tenant.Settings.DistanceUnit = profile.DistanceUnit;
        tenant.Settings.WeightUnit = profile.WeightUnit;
        tenant.Settings.DateFormat = profile.DateFormat;
        tenant.Settings.Timezone = profile.Timezone;
    }
}

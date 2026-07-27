using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Models;

/// <summary>
/// Configuration for a demo tenant to be seeded by the DbMigrator.
/// Loaded from the "Tenants" section of appsettings.json.
/// </summary>
public sealed record DemoTenantConfig
{
    public required string Name { get; init; }
    public required string CompanyName { get; init; }
    public required string BillingEmail { get; init; }
    public required Region Region { get; init; }

    /// <summary>
    /// Top-level section in <c>SeedData/*.json</c> this tenant draws its users, customers and
    /// portal logins from. Defaults to the region name, so two tenants in the same region need
    /// distinct keys - a seed user belongs to exactly one tenant (<c>User.TenantId</c>), and
    /// sharing a key would re-home the other tenant's logins.
    /// </summary>
    public string? SeedDataKey { get; init; }

    public OperatingMode OperatingMode { get; init; } = OperatingMode.Fleet;

    /// <summary>
    /// Multiplier on the fake-data volumes (loads, trips, containers, expenses...). 1.0 is the
    /// fleet-sized demo; a one-truck tenant wants something closer to 0.1. Scaled counts floor at 1.
    /// </summary>
    public double DataScale { get; init; } = 1.0;

    /// <summary>
    /// Optional explicit connection string. When omitted, DemoTenantsSeeder falls back
    /// to <c>ConnectionStrings:{Name}TenantDatabase</c> - the slot Aspire's
    /// WithReference() populates at runtime.
    /// </summary>
    public string? ConnectionString { get; init; }

    public string ResolveSeedDataKey() =>
        string.IsNullOrWhiteSpace(SeedDataKey) ? Region.ToString() : SeedDataKey;
}

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
    /// <c>SeedData/*.json</c> section this tenant draws its users and logins from; defaults to the
    /// region name. Two tenants in the same region need distinct keys or one re-homes the other's logins.
    /// </summary>
    public string? SeedDataKey { get; init; }

    public OperatingMode OperatingMode { get; init; } = OperatingMode.Fleet;

    /// <summary>Multiplier on the fake-data volumes; 1.0 is the fleet-sized demo.</summary>
    public double DataScale { get; init; } = 1.0;

    /// <summary>
    /// Optional explicit connection string. When omitted, DemoTenantsSeeder reads the
    /// <c>ConnectionStrings:*TenantDatabase</c> slot for this tenant name.
    /// </summary>
    public string? ConnectionString { get; init; }

    public string ResolveSeedDataKey() =>
        string.IsNullOrWhiteSpace(SeedDataKey) ? Region.ToString() : SeedDataKey;
}

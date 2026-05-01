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
    /// Optional explicit connection string. When omitted, DemoTenantsSeeder falls back
    /// to <c>ConnectionStrings:{Name}TenantDatabase</c> — the slot Aspire's
    /// WithReference() populates at runtime.
    /// </summary>
    public string? ConnectionString { get; init; }
}

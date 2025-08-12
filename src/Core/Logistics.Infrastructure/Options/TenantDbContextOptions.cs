namespace Logistics.Infrastructure.Options;

public record TenantDbContextOptions
{
    public string DefaultTenantDbConnectionSection { get; set; } = "DefaultTenantDatabase";
    public string TenantsConfigSection { get; set; } = "TenantsDatabaseConfig";
    public string? ConnectionString { get; set; }
}

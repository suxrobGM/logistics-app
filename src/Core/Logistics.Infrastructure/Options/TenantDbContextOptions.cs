namespace Logistics.Infrastructure.Options;

public record TenantDbContextOptions
{
    public string DefaultTenantDbConnectionSection { get; set; } = "DefaultTenantDatabase";
    public string? ConnectionString { get; set; }
}

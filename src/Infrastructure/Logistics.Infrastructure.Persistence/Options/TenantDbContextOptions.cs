namespace Logistics.Infrastructure.Persistence.Options;

public record TenantDbContextOptions
{
    public string DefaultTenantDbConnectionSection { get; set; } = "DefaultTenantDatabase";
    public string? ConnectionString { get; set; }
}

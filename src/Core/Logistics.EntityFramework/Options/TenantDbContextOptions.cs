namespace Logistics.EntityFramework.Options;

public class TenantDbContextOptions
{
    public string? ConnectionString { get; set; }
    public bool UseTenantService { get; set; }
}
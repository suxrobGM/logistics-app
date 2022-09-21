namespace Logistics.Domain.Entities;

public class Tenant : Entity
{
    [StringLength(TenantConsts.NameLength)]
    public string? Name { get; set; }
    
    [StringLength(TenantConsts.DisplayNameLength)]
    public string? DisplayName { get; set; }
    
    [StringLength(TenantConsts.ConnectionStringLength)]
    public string? ConnectionString { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

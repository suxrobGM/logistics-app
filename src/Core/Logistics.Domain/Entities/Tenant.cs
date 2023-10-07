using Logistics.Domain.Abstractions;

namespace Logistics.Domain.Entities;

public class Tenant : AuditableEntity
{
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? ConnectionString { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

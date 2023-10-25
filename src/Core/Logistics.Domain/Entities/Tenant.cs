using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class Tenant : AuditableEntity
{
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? ConnectionString { get; set; }
}

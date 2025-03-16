using Logistics.Domain.Core;
using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class Tenant : Entity
{
    public required string Name { get; set; }
    public string? CompanyName { get; set; }
    public Address CompanyAddress { get; set; } = Address.NullAddress;
    public required string ConnectionString { get; set; }
    public required string BillingEmail { get; set; }
    
    /// <summary>
    /// Subscription ID, if not set, the tenant is on a free plan
    /// </summary>
    public string? SubscriptionId { get; set; }
    public virtual Subscription? Subscription { get; set; }
    
    /// <summary>
    /// Users that belong to this tenant
    /// </summary>
    public virtual List<User> Users { get; } = [];
}

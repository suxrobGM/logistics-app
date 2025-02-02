using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class SubscriptionPlan : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    /// <summary>
    /// Subscription price per employee
    /// </summary>
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public virtual List<Subscription> Subscriptions { get; set; } = new();
}

namespace Logistics.Shared.Models;

public class CreateSubscriptionPlan
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool HasTrial { get; set; }
}

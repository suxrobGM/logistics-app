namespace Logistics.Shared.Models;

public class SubscriptionPlanDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

namespace Logistics.Shared.Models;

public record SubscriptionPlanDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? StripePriceId { get; set; }
    public string? StripeProductId { get; set; }
    public bool HasTrial { get; set; }
}

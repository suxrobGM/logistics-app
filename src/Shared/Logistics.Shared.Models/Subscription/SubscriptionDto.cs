using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record SubscriptionDto
{
    public Guid? Id { get; set; }
    public SubscriptionStatus Status { get; set; }
    public TenantDto? Tenant { get; set; }
    public SubscriptionPlanDto? Plan { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
}

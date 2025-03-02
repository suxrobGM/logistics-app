using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public class SubscriptionDto
{
    public string? Id { get; set; }
    public SubscriptionStatus Status { get; set; }
    public TenantDto? Tenant { get; set; }
    public SubscriptionPlanDto? Plan { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
}

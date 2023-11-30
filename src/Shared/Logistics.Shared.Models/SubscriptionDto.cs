using Logistics.Shared.Enums;

namespace Logistics.Shared.Models;

public class SubscriptionDto
{
    public required string Id { get; set; }
    public SubscriptionStatus Status { get; set; }
    public required TenantDto Tenant { get; set; }
    public required SubscriptionPlanDto Plan { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
}

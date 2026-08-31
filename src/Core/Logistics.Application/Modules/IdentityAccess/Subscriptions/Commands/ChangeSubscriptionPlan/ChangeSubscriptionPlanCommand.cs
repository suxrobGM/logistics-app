using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

public class ChangeSubscriptionPlanCommand : ICommand
{
    public Guid SubscriptionId { get; set; }
    public Guid NewPlanId { get; set; }

    /// <summary>Caller's own JWT tenant claim, set by the controller - null for a platform admin.</summary>
    public Guid? CallerTenantId { get; set; }

    /// <summary>True when the caller is a platform SuperAdmin/Admin, set by the controller.</summary>
    public bool IsPlatformAdmin { get; set; }
}

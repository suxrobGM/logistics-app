using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

/// <summary>Chooses which shipment update emails a customer receives.</summary>
[RequiresFeature(TenantFeature.CustomerUpdates)]
public class SetCustomerUpdatePreferencesCommand : ICommand<Result>
{
    public Guid CustomerId { get; set; }
    public bool Dispatched { get; set; }
    public bool PickedUp { get; set; }
    public bool Delivered { get; set; }
    public bool Late { get; set; }
}

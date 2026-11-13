using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Customers.Commands;

[RequiresFeature(TenantFeature.CustomerUpdates)]
public class SetCustomerUpdatePreferencesCommand : ICommand<Result>
{
    public Guid CustomerId { get; set; }
    public bool Dispatched { get; set; }
    public bool PickedUp { get; set; }
    public bool Delivered { get; set; }
    public bool Late { get; set; }
}

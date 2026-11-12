using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

/// <summary>Emails the customer a short free-text delay notice in the running-late template.</summary>
[RequiresFeature(TenantFeature.CustomerUpdates)]
public class SendCustomerDelayNoticeCommand : ICommand<Result>
{
    public Guid LoadId { get; set; }
    public string Message { get; set; } = "";
}

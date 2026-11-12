using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

/// <summary>Uses the running-late email template.</summary>
[RequiresFeature(TenantFeature.CustomerUpdates)]
public class SendCustomerDelayNoticeCommand : ICommand<Result>
{
    public Guid LoadId { get; set; }
    public string Message { get; set; } = "";
}

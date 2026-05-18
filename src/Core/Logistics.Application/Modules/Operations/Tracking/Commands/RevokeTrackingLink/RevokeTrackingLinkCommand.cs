using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Tracking.Commands;

public class RevokeTrackingLinkCommand : ICommand<Result>
{
    public required Guid Id { get; set; }
}

using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class RevokeTrackingLinkCommand : IAppRequest<Result>
{
    public required Guid Id { get; set; }
}

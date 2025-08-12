using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class ConfirmLoadStatusCommand : IAppRequest
{
    public Guid DriverId { get; set; }
    public Guid LoadId { get; set; }
    public LoadStatus? LoadStatus { get; set; }
}

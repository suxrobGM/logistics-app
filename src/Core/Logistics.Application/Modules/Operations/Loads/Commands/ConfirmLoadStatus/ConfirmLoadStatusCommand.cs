using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public class ConfirmLoadStatusCommand : ICommand
{
    public Guid DriverId { get; set; }
    public Guid LoadId { get; set; }
    public LoadStatus? LoadStatus { get; set; }
}

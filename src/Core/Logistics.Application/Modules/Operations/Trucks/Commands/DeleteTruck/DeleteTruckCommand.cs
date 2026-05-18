using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

public class DeleteTruckCommand : ICommand
{
    public Guid Id { get; set; }
}

using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

public class DeleteTripCommand : ICommand, IHaveId
{
    public Guid Id { get; set; }
}

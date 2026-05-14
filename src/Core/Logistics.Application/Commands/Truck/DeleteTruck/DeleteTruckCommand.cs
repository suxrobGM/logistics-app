using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteTruckCommand : ICommand
{
    public Guid Id { get; set; }
}

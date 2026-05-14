using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteTripCommand : ICommand
{
    public Guid Id { get; set; }
}

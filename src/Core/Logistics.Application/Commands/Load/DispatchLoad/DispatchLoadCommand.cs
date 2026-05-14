using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DispatchLoadCommand : ICommand
{
    public Guid Id { get; set; }
}

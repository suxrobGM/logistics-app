using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteLoadCommand : ICommand
{
    public Guid Id { get; set; }
}

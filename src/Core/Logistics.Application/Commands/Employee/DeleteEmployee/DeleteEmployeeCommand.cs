using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteEmployeeCommand : ICommand
{
    public Guid UserId { get; set; }
}

using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

public class DeleteEmployeeCommand : ICommand
{
    public Guid UserId { get; set; }
}


namespace Logistics.Application.Modules.Identity.Employees.Commands;

public class DeleteEmployeeCommand : ICommand
{
    public Guid UserId { get; set; }
}

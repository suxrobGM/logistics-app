using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

public class SetupEmployeePayoutCommand : ICommand
{
    public Guid EmployeeId { get; set; }
}

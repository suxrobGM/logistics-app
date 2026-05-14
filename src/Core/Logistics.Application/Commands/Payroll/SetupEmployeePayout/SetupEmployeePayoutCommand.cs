using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class SetupEmployeePayoutCommand : ICommand
{
    public Guid EmployeeId { get; set; }
}

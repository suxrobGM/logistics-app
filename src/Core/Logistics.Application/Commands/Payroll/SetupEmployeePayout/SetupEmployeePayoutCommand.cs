using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class SetupEmployeePayoutCommand : IAppRequest
{
    public Guid EmployeeId { get; set; }
}

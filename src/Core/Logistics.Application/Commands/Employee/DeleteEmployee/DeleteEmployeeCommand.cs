using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteEmployeeCommand : IAppRequest
{
    public Guid UserId { get; set; }
}

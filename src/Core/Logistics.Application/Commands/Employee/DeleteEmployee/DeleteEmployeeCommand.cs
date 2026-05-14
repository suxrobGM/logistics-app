using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteEmployeeCommand : ICrossDatabaseCommand
{
    public Guid UserId { get; set; }
}

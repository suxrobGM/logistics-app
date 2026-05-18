using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Privacy.Commands;

public class RequestDataDeletionCommand : ICommand<Result<Guid>>
{
    public string? Reason { get; set; }
}

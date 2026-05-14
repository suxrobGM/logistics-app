using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class RequestDataDeletionCommand : ICommand<Result<Guid>>
{
    public string? Reason { get; set; }
}

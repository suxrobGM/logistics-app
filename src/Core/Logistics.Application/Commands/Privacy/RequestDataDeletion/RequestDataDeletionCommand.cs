using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class RequestDataDeletionCommand : IAppRequest<Result<Guid>>
{
    public string? Reason { get; set; }
}

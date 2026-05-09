using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class CancelDataDeletionCommand : IAppRequest
{
    public Guid Id { get; set; }
}

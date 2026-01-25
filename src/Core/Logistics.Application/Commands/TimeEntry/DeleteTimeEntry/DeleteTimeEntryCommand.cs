using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class DeleteTimeEntryCommand : IAppRequest<Result>
{
    public required Guid Id { get; set; }
}

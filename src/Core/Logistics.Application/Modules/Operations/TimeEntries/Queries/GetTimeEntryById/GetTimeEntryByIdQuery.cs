using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.TimeEntries.Queries;

public class GetTimeEntryByIdQuery : IQuery<Result<TimeEntryDto>>
{
    public required Guid Id { get; init; }
}

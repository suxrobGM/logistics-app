using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTimeEntryByIdQuery : IAppRequest<Result<TimeEntryDto>>
{
    public required Guid Id { get; init; }
}

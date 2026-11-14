using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.TimeEntries.Queries;

public class GetTimeEntriesQuery : SearchableQuery, IQuery<PagedResult<TimeEntryDto>>
{
    public Guid? EmployeeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public TimeEntryType? Type { get; set; }
}

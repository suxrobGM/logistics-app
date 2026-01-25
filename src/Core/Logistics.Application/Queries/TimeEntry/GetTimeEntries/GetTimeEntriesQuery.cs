using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTimeEntriesQuery : SearchableQuery, IAppRequest<PagedResult<TimeEntryDto>>
{
    public Guid? EmployeeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Type { get; set; }
    public bool? IncludeLinkedToPayroll { get; set; }
}

using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.TimeEntries.Commands;

[RequiresFeature(TenantFeature.Timesheets)]
public class CreateTimeEntryCommand : ICommand<Result<Guid>>
{
    public required Guid EmployeeId { get; set; }
    public required DateTime StartedAt { get; set; }
    public required DateTime EndedAt { get; set; }
    public decimal? TotalHours { get; set; }
    public TimeEntryType Type { get; set; } = TimeEntryType.Regular;
    public string? Notes { get; set; }
}

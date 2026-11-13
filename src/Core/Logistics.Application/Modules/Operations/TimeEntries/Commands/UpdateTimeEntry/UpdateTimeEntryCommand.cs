using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.TimeEntries.Commands;

[RequiresFeature(TenantFeature.Timesheets)]
public class UpdateTimeEntryCommand : ICommand<Result>
{
    public required Guid Id { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public decimal? TotalHours { get; set; }
    public TimeEntryType? Type { get; set; }
    public string? Notes { get; set; }
}

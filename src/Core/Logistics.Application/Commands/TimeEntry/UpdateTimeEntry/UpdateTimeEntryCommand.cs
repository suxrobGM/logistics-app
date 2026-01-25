using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class UpdateTimeEntryCommand : IAppRequest<Result>
{
    public required Guid Id { get; set; }
    public DateTime? Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public decimal? TotalHours { get; set; }
    public TimeEntryType? Type { get; set; }
    public string? Notes { get; set; }
}

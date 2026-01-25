using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class CreateTimeEntryCommand : IAppRequest<Result<Guid>>
{
    public required Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal? TotalHours { get; set; }
    public TimeEntryType Type { get; set; } = TimeEntryType.Regular;
    public string? Notes { get; set; }
}

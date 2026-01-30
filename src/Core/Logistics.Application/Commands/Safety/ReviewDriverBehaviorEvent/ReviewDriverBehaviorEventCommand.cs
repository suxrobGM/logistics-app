using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record ReviewDriverBehaviorEventCommand : IAppRequest<Result<DriverBehaviorEventDto>>
{
    public required Guid Id { get; set; }
    public string? ReviewNotes { get; set; }
    public bool IsDismissed { get; set; }
}

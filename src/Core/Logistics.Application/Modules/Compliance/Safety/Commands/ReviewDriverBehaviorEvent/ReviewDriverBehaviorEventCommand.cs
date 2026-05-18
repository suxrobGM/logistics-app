using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Safety.Commands;

[RequiresFeature(TenantFeature.Safety)]
public record ReviewDriverBehaviorEventCommand : ICommand<Result<DriverBehaviorEventDto>>
{
    public required Guid Id { get; set; }
    public string? ReviewNotes { get; set; }
    public bool IsDismissed { get; set; }
}

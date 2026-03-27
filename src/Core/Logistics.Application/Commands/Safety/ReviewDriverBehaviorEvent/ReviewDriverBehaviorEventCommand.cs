using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.Safety)]
public record ReviewDriverBehaviorEventCommand : IAppRequest<Result<DriverBehaviorEventDto>>
{
    public required Guid Id { get; set; }
    public string? ReviewNotes { get; set; }
    public bool IsDismissed { get; set; }
}

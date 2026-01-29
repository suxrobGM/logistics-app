using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record ResolveEmergencyAlertCommand(
    Guid AlertId,
    Guid ResolvedById,
    string ResolutionNotes,
    bool IsFalseAlarm = false
) : IAppRequest<Result<EmergencyAlertDto>>;

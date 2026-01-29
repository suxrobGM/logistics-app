using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record AcknowledgeEmergencyAlertCommand(
    Guid AlertId,
    Guid AcknowledgedById,
    string? Notes
) : IAppRequest<Result<EmergencyAlertDto>>;

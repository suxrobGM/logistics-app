using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class AcknowledgeEmergencyAlertHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<AcknowledgeEmergencyAlertCommand, Result<EmergencyAlertDto>>
{
    public async Task<Result<EmergencyAlertDto>> Handle(AcknowledgeEmergencyAlertCommand req, CancellationToken ct)
    {
        var alert = await tenantUow.Repository<EmergencyAlert>().GetByIdAsync(req.AlertId, ct);
        if (alert is null)
        {
            return Result<EmergencyAlertDto>.Fail("Emergency alert not found.");
        }

        if (alert.Status != EmergencyAlertStatus.Active)
        {
            return Result<EmergencyAlertDto>.Fail("Only active alerts can be acknowledged.");
        }

        alert.Status = EmergencyAlertStatus.Acknowledged;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.AcknowledgedById = req.AcknowledgedById;

        tenantUow.Repository<EmergencyAlert>().Update(alert);
        await tenantUow.SaveChangesAsync(ct);

        return Result<EmergencyAlertDto>.Ok(alert.ToDto());
    }
}

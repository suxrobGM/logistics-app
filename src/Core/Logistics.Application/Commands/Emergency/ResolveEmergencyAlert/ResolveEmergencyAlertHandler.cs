using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class ResolveEmergencyAlertHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<ResolveEmergencyAlertCommand, Result<EmergencyAlertDto>>
{
    public async Task<Result<EmergencyAlertDto>> Handle(ResolveEmergencyAlertCommand req, CancellationToken ct)
    {
        var alert = await tenantUow.Repository<EmergencyAlert>().GetByIdAsync(req.AlertId, ct);
        if (alert is null)
        {
            return Result<EmergencyAlertDto>.Fail("Emergency alert not found.");
        }

        if (alert.Status == EmergencyAlertStatus.Resolved || alert.Status == EmergencyAlertStatus.FalseAlarm)
        {
            return Result<EmergencyAlertDto>.Fail("Alert is already resolved.");
        }

        alert.Status = req.IsFalseAlarm ? EmergencyAlertStatus.FalseAlarm : EmergencyAlertStatus.Resolved;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolvedById = req.ResolvedById;
        alert.ResolutionNotes = req.ResolutionNotes;

        tenantUow.Repository<EmergencyAlert>().Update(alert);
        await tenantUow.SaveChangesAsync(ct);

        return Result<EmergencyAlertDto>.Ok(alert.ToDto());
    }
}

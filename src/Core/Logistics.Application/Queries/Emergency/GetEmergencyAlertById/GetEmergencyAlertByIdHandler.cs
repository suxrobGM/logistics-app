using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetEmergencyAlertByIdHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetEmergencyAlertByIdQuery, Result<EmergencyAlertDto>>
{
    public async Task<Result<EmergencyAlertDto>> Handle(GetEmergencyAlertByIdQuery req, CancellationToken ct)
    {
        var alert = await tenantUow.Repository<EmergencyAlert>().GetByIdAsync(req.Id, ct);

        if (alert is null)
        {
            return Result<EmergencyAlertDto>.Fail("Emergency alert not found.");
        }

        return Result<EmergencyAlertDto>.Ok(alert.ToDto());
    }
}

using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class TriggerEmergencyAlertHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<TriggerEmergencyAlertCommand, Result<EmergencyAlertDto>>
{
    public async Task<Result<EmergencyAlertDto>> Handle(TriggerEmergencyAlertCommand req, CancellationToken ct)
    {
        var driver = await tenantUow.Repository<Employee>().GetByIdAsync(req.DriverId, ct);
        if (driver is null)
        {
            return Result<EmergencyAlertDto>.Fail("Driver not found.");
        }

        var alert = new EmergencyAlert
        {
            DriverId = req.DriverId,
            TruckId = req.TruckId,
            TripId = req.TripId,
            AlertType = req.Type,
            Source = req.Source,
            Status = EmergencyAlertStatus.Active,
            TriggeredAt = DateTime.UtcNow,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            Address = req.Address,
            Description = req.Description
        };

        await tenantUow.Repository<EmergencyAlert>().AddAsync(alert, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<EmergencyAlertDto>.Ok(alert.ToDto());
    }
}

using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateAccidentReportHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateAccidentReportCommand, Result<AccidentReportDto>>
{
    public async Task<Result<AccidentReportDto>> Handle(CreateAccidentReportCommand req, CancellationToken ct)
    {
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result<AccidentReportDto>.Fail("Truck not found.");
        }

        var driver = await tenantUow.Repository<Employee>().GetByIdAsync(req.DriverId, ct);
        if (driver is null)
        {
            return Result<AccidentReportDto>.Fail("Driver not found.");
        }

        var report = new AccidentReport
        {
            TruckId = req.TruckId,
            Truck = truck,
            DriverId = req.DriverId,
            Driver = driver,
            AccidentType = req.Type,
            Severity = req.Severity,
            Status = AccidentReportStatus.Draft,
            AccidentDateTime = req.AccidentDateTime,
            Address = req.Location,
            Latitude = req.Latitude ?? 0,
            Longitude = req.Longitude ?? 0,
            Description = req.Description,
            WeatherConditions = req.WeatherConditions,
            RoadConditions = req.RoadConditions,
            AnyInjuries = req.InjuriesReported,
            NumberOfInjuries = req.NumberOfInjuries,
            InjuryDescription = req.InjuryDescription,
            EstimatedDamageCost = req.EstimatedDamage,
            VehicleDamageDescription = req.DamageDescription,
            TripId = req.TripId
        };

        await tenantUow.Repository<AccidentReport>().AddAsync(report, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<AccidentReportDto>.Ok(report.ToDto());
    }
}

using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Safety.Commands;

internal sealed class UpdateAccidentReportHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<UpdateAccidentReportCommand, Result<AccidentReportDto>>
{
    public async Task<Result<AccidentReportDto>> Handle(UpdateAccidentReportCommand req, CancellationToken ct)
    {
        var report = await tenantUow.Repository<AccidentReport>().GetByIdAsync(req.Id, ct);
        if (report is null)
        {
            return Result<AccidentReportDto>.Fail("Accident report not found.", ErrorCodes.NotFound);
        }

        if (report.Status != AccidentReportStatus.Draft)
        {
            return Result<AccidentReportDto>.Fail("Only draft reports can be edited.");
        }

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result<AccidentReportDto>.Fail("Truck not found.", ErrorCodes.NotFound);
        }

        var driver = await tenantUow.Repository<Employee>().GetByIdAsync(req.DriverId, ct);
        if (driver is null)
        {
            return Result<AccidentReportDto>.Fail("Driver not found.", ErrorCodes.NotFound);
        }

        report.TruckId = req.TruckId;
        report.Truck = truck;
        report.DriverId = req.DriverId;
        report.Driver = driver;
        report.AccidentType = req.Type;
        report.Severity = req.Severity;
        report.AccidentDateTime = req.AccidentDateTime;
        report.Address = req.Location;
        report.Latitude = req.Latitude ?? 0;
        report.Longitude = req.Longitude ?? 0;
        report.Description = req.Description;
        report.WeatherConditions = req.WeatherConditions;
        report.RoadConditions = req.RoadConditions;
        report.AnyInjuries = req.InjuriesReported;
        report.NumberOfInjuries = req.NumberOfInjuries;
        report.InjuryDescription = req.InjuryDescription;
        report.FatalitiesReported = req.FatalitiesReported;
        report.NumberOfFatalities = req.FatalitiesReported ? req.NumberOfFatalities : null;
        report.VehicleTowed = req.VehicleTowed;
        report.TowCompany = req.TowCompany;
        report.HazmatInvolved = req.HazmatInvolved;
        report.HazmatDescription = req.HazmatDescription;
        report.EstimatedDamageCost = req.EstimatedDamage;
        report.VehicleDamageDescription = req.DamageDescription;
        report.TripId = req.TripId;

        await tenantUow.SaveChangesAsync(ct);

        return Result<AccidentReportDto>.Ok(report.ToDto());
    }
}

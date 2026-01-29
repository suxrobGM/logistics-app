using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateDvirReportHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateDvirReportCommand, Result<DvirReportDto>>
{
    public async Task<Result<DvirReportDto>> Handle(CreateDvirReportCommand req, CancellationToken ct)
    {
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result<DvirReportDto>.Fail("Truck not found.");
        }

        var driver = await tenantUow.Repository<Employee>().GetByIdAsync(req.DriverId, ct);
        if (driver is null)
        {
            return Result<DvirReportDto>.Fail("Driver not found.");
        }

        var report = new DvirReport
        {
            TruckId = req.TruckId,
            Truck = truck,
            DriverId = req.DriverId,
            Driver = driver,
            Type = req.Type,
            Status = DvirStatus.Draft,
            InspectionDate = DateTime.UtcNow,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            OdometerReading = req.OdometerReading,
            DriverNotes = req.DriverNotes,
            DriverSignature = req.DriverSignature,
            TripId = req.TripId,
            HasDefects = req.Defects.Count > 0
        };

        foreach (var defectDto in req.Defects)
        {
            var defect = new DvirDefect
            {
                DvirReport = report,
                Category = defectDto.Category,
                Description = defectDto.Description,
                Severity = defectDto.Severity,
                IsCorrected = false
            };
            report.Defects.Add(defect);
        }

        await tenantUow.Repository<DvirReport>().AddAsync(report, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<DvirReportDto>.Ok(report.ToDto());
    }
}

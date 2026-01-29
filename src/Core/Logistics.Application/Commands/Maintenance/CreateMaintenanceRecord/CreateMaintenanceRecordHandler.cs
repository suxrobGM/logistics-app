using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateMaintenanceRecordHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateMaintenanceRecordCommand, Result<MaintenanceRecordDto>>
{
    public async Task<Result<MaintenanceRecordDto>> Handle(CreateMaintenanceRecordCommand req, CancellationToken ct)
    {
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result<MaintenanceRecordDto>.Fail("Truck not found.");
        }

        Employee? performedBy = null;
        if (req.PerformedById.HasValue)
        {
            performedBy = await tenantUow.Repository<Employee>().GetByIdAsync(req.PerformedById.Value, ct);
        }

        var record = new MaintenanceRecord
        {
            TruckId = req.TruckId,
            Truck = truck,
            MaintenanceType = req.Type,
            Description = req.Description,
            ServiceDate = req.ServiceDate,
            OdometerReading = (int)req.OdometerReading,
            EngineHours = req.EngineHours,
            VendorName = req.VendorName,
            InvoiceNumber = req.InvoiceNumber,
            LaborCost = req.LaborCost,
            PartsCost = req.PartsCost,
            TotalCost = req.LaborCost + req.PartsCost,
            PerformedById = req.PerformedById,
            PerformedBy = performedBy,
            WorkPerformed = req.Notes
        };

        foreach (var partDto in req.Parts)
        {
            var part = new MaintenancePart
            {
                MaintenanceRecord = record,
                PartNumber = partDto.PartNumber,
                PartName = partDto.PartName,
                Quantity = partDto.Quantity,
                UnitCost = partDto.UnitCost,
                TotalCost = partDto.Quantity * partDto.UnitCost
            };
            record.Parts.Add(part);
            record.PartsCost += part.TotalCost;
        }

        record.TotalCost = record.LaborCost + record.PartsCost;

        await tenantUow.Repository<MaintenanceRecord>().AddAsync(record, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<MaintenanceRecordDto>.Ok(record.ToDto());
    }
}

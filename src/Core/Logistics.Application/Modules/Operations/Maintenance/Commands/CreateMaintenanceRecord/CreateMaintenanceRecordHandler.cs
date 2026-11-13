using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Maintenance.Commands;

internal sealed class CreateMaintenanceRecordHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<CreateMaintenanceRecordCommand, Result<MaintenanceRecordDto>>
{
    public async Task<Result<MaintenanceRecordDto>> Handle(CreateMaintenanceRecordCommand req, CancellationToken ct)
    {
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result<MaintenanceRecordDto>.Fail("Truck not found.", ErrorCodes.NotFound);
        }

        Employee? performedBy = null;
        if (req.PerformedById.HasValue)
        {
            performedBy = await tenantUow.Repository<Employee>().GetByIdAsync(req.PerformedById.Value, ct);
            if (performedBy is null)
            {
                return Result<MaintenanceRecordDto>.Fail("Performed by employee not found.", ErrorCodes.NotFound);
            }
        }

        var record = new MaintenanceRecord
        {
            TruckId = req.TruckId,
            Truck = truck,
            MaintenanceType = req.Type,
            Description = req.Description,
            ServiceDate = req.ServiceDate,
            OdometerReading = req.OdometerReading ?? 0,
            EngineHours = req.EngineHours,
            VendorName = req.VendorName,
            InvoiceNumber = req.InvoiceNumber,
            LaborCost = req.LaborCost,
            PartsCost = req.PartsCost,
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
        }

        // Itemized parts replace the lump-sum figure rather than adding to it.
        if (record.Parts.Count > 0)
        {
            record.PartsCost = record.Parts.Sum(p => p.TotalCost);
        }

        record.TotalCost = record.LaborCost + record.PartsCost;

        await tenantUow.Repository<MaintenanceRecord>().AddAsync(record, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<MaintenanceRecordDto>.Ok(record.ToDto());
    }
}

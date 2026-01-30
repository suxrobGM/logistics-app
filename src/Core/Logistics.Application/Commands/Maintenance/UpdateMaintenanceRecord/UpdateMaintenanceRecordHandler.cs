using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateMaintenanceRecordHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateMaintenanceRecordCommand, Result<MaintenanceRecordDto>>
{
    public async Task<Result<MaintenanceRecordDto>> Handle(UpdateMaintenanceRecordCommand req, CancellationToken ct)
    {
        var record = await tenantUow.Repository<MaintenanceRecord>().GetByIdAsync(req.Id, ct);

        if (record is null)
        {
            return Result<MaintenanceRecordDto>.Fail($"Could not find maintenance record with ID '{req.Id}'");
        }

        // Verify truck exists if changing
        if (record.TruckId != req.TruckId)
        {
            var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
            if (truck is null)
            {
                return Result<MaintenanceRecordDto>.Fail("Truck not found.");
            }
            record.TruckId = req.TruckId;
            record.Truck = truck;
        }

        // Verify performed by employee exists if specified
        if (req.PerformedById.HasValue && record.PerformedById != req.PerformedById)
        {
            var performedBy = await tenantUow.Repository<Employee>().GetByIdAsync(req.PerformedById.Value, ct);
            if (performedBy is null)
            {
                return Result<MaintenanceRecordDto>.Fail("Performed by employee not found.");
            }
            record.PerformedById = req.PerformedById;
            record.PerformedBy = performedBy;
        }
        else if (!req.PerformedById.HasValue)
        {
            record.PerformedById = null;
            record.PerformedBy = null;
        }

        // Update fields
        record.MaintenanceType = req.Type;
        record.Description = req.Description;
        record.ServiceDate = req.ServiceDate;
        record.OdometerReading = req.OdometerReading ?? 0;
        record.EngineHours = req.EngineHours;
        record.VendorName = req.VendorName;
        record.InvoiceNumber = req.InvoiceNumber;
        record.LaborCost = req.LaborCost;
        record.PartsCost = req.PartsCost;
        record.TotalCost = req.LaborCost + req.PartsCost;
        record.WorkPerformed = req.Notes;

        tenantUow.Repository<MaintenanceRecord>().Update(record);
        await tenantUow.SaveChangesAsync(ct);

        return Result<MaintenanceRecordDto>.Ok(record.ToDto());
    }
}

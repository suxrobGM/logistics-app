using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Inspections.Queries;

internal sealed class GetConditionReportsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetConditionReportsQuery, Result<List<ConditionReportDto>>>
{
    public async Task<Result<List<ConditionReportDto>>> Handle(GetConditionReportsQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<LoadConditionReport>().Query();

        if (req.LoadId.HasValue)
        {
            query = query.Where(r => r.LoadId == req.LoadId.Value);
        }

        if (req.ConditionReportId.HasValue)
        {
            query = query.Where(r => r.Id == req.ConditionReportId.Value);
        }

        var reports = await query
            .OrderByDescending(r => r.InspectedAt)
            .ToListAsync(ct);

        var dtos = new List<ConditionReportDto>();

        foreach (var report in reports)
        {
            // Photos are stored as DeliveryDocument rows linked by LoadId + inspection
            // timestamp proximity (legacy pattern — out of scope to refactor here).
            var photos = await tenantUow.Repository<DeliveryDocument>().Query()
                .Where(d => d.LoadId == report.LoadId &&
                    (d.Type == DocumentType.PickupInspection || d.Type == DocumentType.DeliveryInspection) &&
                    d.CapturedAt != null &&
                    d.CapturedAt >= report.InspectedAt.AddMinutes(-5) &&
                    d.CapturedAt <= report.InspectedAt.AddMinutes(5))
                .ToListAsync(ct);

            var dto = new ConditionReportDto
            {
                Id = report.Id,
                LoadId = report.LoadId,
                LoadReferenceId = report.Load?.Number.ToString(),
                LoadType = report.Load?.Type ?? LoadType.GeneralFreight,
                Type = report.Type,
                Vin = report.Vin,
                VehicleYear = report.VehicleYear,
                VehicleMake = report.VehicleMake,
                VehicleModel = report.VehicleModel,
                VehicleBodyClass = report.VehicleBodyClass,
                ContainerNumber = report.ContainerNumber,
                SealNumber = report.SealNumber,
                Notes = report.Notes,
                HasSignature = !string.IsNullOrEmpty(report.InspectorSignature),
                Latitude = report.Latitude,
                Longitude = report.Longitude,
                InspectedAt = report.InspectedAt,
                CreatedAt = report.CreatedAt,
                InspectedById = report.InspectedById,
                InspectorName = report.InspectedBy?.GetFullName(),
                Defects = [.. report.Defects.Select(d => d.ToDto())],
                Photos = [.. photos.Select(p => p.ToDto())]
            };

            dtos.Add(dto);
        }

        return Result<List<ConditionReportDto>>.Ok(dtos);
    }
}

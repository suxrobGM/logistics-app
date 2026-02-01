using System.Text.Json;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetConditionReportsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetConditionReportsQuery, Result<List<ConditionReportDto>>>
{
    public async Task<Result<List<ConditionReportDto>>> Handle(GetConditionReportsQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<VehicleConditionReport>().Query()
            .AsQueryable();

        if (req.LoadId.HasValue)
        {
            query = query.Where(r => r.LoadId == req.LoadId.Value);
        }

        if (req.VehicleConditionReportId.HasValue)
        {
            query = query.Where(r => r.Id == req.VehicleConditionReportId.Value);
        }

        var reports = await query
            .OrderByDescending(r => r.InspectedAt)
            .ToListAsync(ct);

        var dtos = new List<ConditionReportDto>();

        foreach (var report in reports)
        {
            // Get associated photos (inspection photos are stored as DeliveryDocuments with capture metadata)
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
                Vin = report.Vin,
                Type = report.Type,
                VehicleYear = report.VehicleYear,
                VehicleMake = report.VehicleMake,
                VehicleModel = report.VehicleModel,
                VehicleBodyClass = report.VehicleBodyClass,
                Notes = report.Notes,
                HasSignature = !string.IsNullOrEmpty(report.InspectorSignature),
                Latitude = report.Latitude,
                Longitude = report.Longitude,
                InspectedAt = report.InspectedAt,
                CreatedAt = report.CreatedAt,
                InspectedById = report.InspectedById,
                InspectorName = report.InspectedBy?.GetFullName(),
                DamageMarkers = ParseDamageMarkers(report.DamageMarkersJson),
                Photos = [.. photos.Select(p => p.ToDto())]
            };

            dtos.Add(dto);
        }

        return Result<List<ConditionReportDto>>.Ok(dtos);
    }

    private static List<DamageMarkerDto> ParseDamageMarkers(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<DamageMarkerDto>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}

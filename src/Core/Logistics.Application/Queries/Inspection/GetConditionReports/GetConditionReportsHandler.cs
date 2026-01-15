using System.Text.Json;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetConditionReportsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetConditionReportsQuery, Result<List<ConditionReportDto>>>
{
    public async Task<Result<List<ConditionReportDto>>> Handle(GetConditionReportsQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<VehicleConditionReport>().Query()
            .Include(r => r.InspectedBy)
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
            // Get associated photos
            var photos = await tenantUow.Repository<LoadDocument>().Query()
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
                Photos = photos.Select(p => new DocumentDto
                {
                    Id = p.Id,
                    FileName = p.FileName,
                    OriginalFileName = p.OriginalFileName,
                    ContentType = p.ContentType,
                    FileSizeBytes = p.FileSizeBytes,
                    BlobPath = p.BlobPath,
                    BlobContainer = p.BlobContainer,
                    Type = p.Type,
                    Status = p.Status,
                    UploadedById = p.UploadedById,
                    LoadId = p.LoadId,
                    CreatedAt = p.CreatedAt
                }).ToList()
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

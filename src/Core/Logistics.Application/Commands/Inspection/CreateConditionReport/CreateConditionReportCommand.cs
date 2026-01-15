using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class CreateConditionReportCommand : IAppRequest<Result<Guid>>
{
    public required Guid LoadId { get; set; }
    public required string Vin { get; set; }
    public required InspectionType Type { get; set; }

    // Optional vehicle info (can be pre-populated from VIN decode)
    public int? VehicleYear { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleBodyClass { get; set; }

    // Damage markers JSON
    public string? DamageMarkersJson { get; set; }

    public string? Notes { get; set; }
    public string? SignatureBase64 { get; set; }

    // Photos
    public List<FileUpload> Photos { get; set; } = new();

    // Location
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Inspector
    public required Guid InspectedById { get; set; }
}

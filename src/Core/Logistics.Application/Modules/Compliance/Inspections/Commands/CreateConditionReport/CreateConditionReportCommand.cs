using System.Text.Json;
using System.Text.Json.Serialization;
using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Inspections.Commands;

[RequiresFeature(TenantFeature.Safety)]
public class CreateConditionReportCommand : ICommand<Result<Guid>>
{
    public required Guid LoadId { get; set; }
    public required InspectionType Type { get; set; }

    // Vehicle-cargo identifier (only required when load is LoadType.Vehicle)
    public string? Vin { get; set; }
    public int? VehicleYear { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleBodyClass { get; set; }

    // Container-cargo identifier (only used for container LoadTypes)
    public string? ContainerNumber { get; set; }
    public string? SealNumber { get; set; }

    /// <summary>
    /// Defects documented during inspection. Server-side, each defect's
    /// <see cref="ConditionDefectInput.PartCategory"/> is validated against the
    /// catalog returned by <c>CargoInspectionPartCategoryExtensions.GetCatalogFor(load.Type)</c>.
    /// </summary>
    public List<ConditionDefectInput> Defects { get; set; } = [];

    public string? Notes { get; set; }
    public string? SignatureBase64 { get; set; }

    public List<FileUpload> Photos { get; set; } = [];

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public required Guid InspectedById { get; set; }
}

public class ConditionDefectInput
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public required CargoInspectionPartCategory PartCategory { get; set; }
    public required string Description { get; set; }
    public required DefectSeverity Severity { get; set; }

    public static List<ConditionDefectInput> ParseDefects(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<ConditionDefectInput>>(json, JsonSerializerOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }
}

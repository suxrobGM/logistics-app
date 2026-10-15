using System.Text.Json;
using System.Text.Json.Serialization;
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

    /// <summary>
    /// The load vehicle being inspected. Required on a vehicle load that lists its vehicles; its
    /// VIN / year / make / model fill any identifier field left empty below.
    /// </summary>
    public Guid? LoadVehicleId { get; set; }

    // Vehicle-cargo identifier (only used when load is LoadType.Vehicle)
    public string? Vin { get; set; }
    public int? VehicleYear { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleBodyClass { get; set; }

    // Container-cargo identifier (only used for container LoadTypes)
    public string? ContainerNumber { get; set; }
    public string? SealNumber { get; set; }

    /// <summary>
    /// Defects documented during inspection. Server-side, each defect's part category is validated
    /// against <c>CargoInspectionPartCategoryExtensions.GetCatalogFor(load.Type)</c>.
    /// </summary>
    public List<ConditionDefectInput> Defects { get; set; } = [];

    public string? Notes { get; set; }
    public string? SignatureBase64 { get; set; }

    public List<FileUpload> Photos { get; set; } = [];

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public required Guid InspectedById { get; set; }
}

/// <summary>
/// A defect from the part list (<see cref="PartCategory"/> + <see cref="Description"/>) or from the
/// vehicle diagram (<see cref="Zone"/> + <see cref="DamageType"/>; the part category is derived from
/// the zone and the description is optional).
/// </summary>
public class ConditionDefectInput
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public CargoInspectionPartCategory? PartCategory { get; set; }
    public string? Description { get; set; }
    public required DefectSeverity Severity { get; set; }

    public VehicleDamageZone? Zone { get; set; }
    public VehicleDamageType? DamageType { get; set; }

    /// <summary>
    /// Index into the report's <see cref="CreateConditionReportCommand.Photos"/> of the photo that
    /// shows this defect.
    /// </summary>
    public int? PhotoIndex { get; set; }

    /// <summary>
    /// The part category this defect is filed under: the diagram zone's category when a zone is set.
    /// </summary>
    public CargoInspectionPartCategory? ResolvePartCategory() => Zone?.ToPartCategory() ?? PartCategory;

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

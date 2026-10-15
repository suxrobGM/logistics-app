using Logistics.Domain.Primitives.Enums;
using static Logistics.Application.Modules.Platform.DataImport.Constants.ImportFieldKeys;

namespace Logistics.Application.Modules.Platform.DataImport.Constants;

/// <summary>
///     Source-system header maps, checked before the synonym table. To add one when a pilot sends a
///     sample export: add an <see cref="ImportPreset"/> to <see cref="All"/> with the export's exact
///     headers per entity. Only list headers you have seen in a real export or a published template -
///     anything else is already covered by <see cref="ImportFieldCatalog"/>'s synonyms.
/// </summary>
internal static class ImportPresets
{
    public const string Generic = "generic";
    public const string Datatruck = "datatruck";

    public static readonly IReadOnlyList<ImportPreset> All =
    [
        new(Generic, "Spreadsheet / other TMS", new Dictionary<ImportEntityType, IReadOnlyDictionary<string, string>>()),

        // Datatruck's published customer import columns and its truck/trailer form fields, all snake_case.
        // Driver headers are not public, so drivers fall back to the synonyms.
        new(Datatruck, "Datatruck", new Dictionary<ImportEntityType, IReadOnlyDictionary<string, string>>
        {
            [ImportEntityType.Customers] = new Dictionary<string, string>
            {
                ["company_name"] = Name,
                ["address1"] = Street,
                ["address2"] = Street2,
                ["city"] = City,
                ["state"] = State,
                ["zip_code"] = Zip
            },
            [ImportEntityType.Trucks] = new Dictionary<string, string>
            {
                ["unit_number"] = Number,
                ["make"] = Make,
                ["model"] = Model,
                ["year"] = Year,
                ["vin"] = Vin,
                ["plate_number"] = LicensePlate,
                ["state"] = LicensePlateState
            },
            [ImportEntityType.Trailers] = new Dictionary<string, string>
            {
                ["unit_number"] = Number,
                ["trailer_type"] = ImportFieldKeys.Type,
                ["make"] = Make,
                ["model"] = Model,
                ["year"] = Year,
                ["vin"] = Vin,
                ["plate_number"] = LicensePlate,
                ["plate_state"] = LicensePlateState
            }
        })
    ];

    public static ImportPreset? Find(string? id) =>
        All.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));
}

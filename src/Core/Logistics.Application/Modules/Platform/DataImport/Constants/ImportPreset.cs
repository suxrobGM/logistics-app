using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Platform.DataImport.Constants;

/// <param name="Headers">Per entity, the source header (matched after normalisation) to its field key.</param>
internal sealed record ImportPreset(
    string Id,
    string Name,
    IReadOnlyDictionary<ImportEntityType, IReadOnlyDictionary<string, string>> Headers);

using Logistics.Application.Modules.Platform.DataImport.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Queries;

public class PreviewImportQuery : IQuery<Result<ImportPreviewDto>>, IImportFile
{
    public required Stream Content { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public long FileSizeBytes { get; init; }
    public ImportEntityType EntityType { get; init; }

    /// <summary>
    ///     A source preset id from <c>GET /imports/presets</c>; null uses the synonyms only.
    /// </summary>
    public string? Preset { get; init; }
}

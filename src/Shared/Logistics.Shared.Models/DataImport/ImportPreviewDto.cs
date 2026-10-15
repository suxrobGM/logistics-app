using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record ImportPreviewDto
{
    public ImportEntityType EntityType { get; init; }
    public IReadOnlyList<string> Headers { get; init; } = [];
    public IReadOnlyList<ImportFieldDto> Fields { get; init; } = [];
    public IReadOnlyList<ImportColumnMappingDto> Mapping { get; init; } = [];

    /// <summary>
    ///     The first data rows, cells aligned to <see cref="Headers"/>.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string?>> PreviewRows { get; init; } = [];

    public int TotalRows { get; init; }
}

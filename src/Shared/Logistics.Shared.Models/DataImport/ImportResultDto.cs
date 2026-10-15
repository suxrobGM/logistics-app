using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record ImportResultDto
{
    public ImportEntityType EntityType { get; init; }

    /// <summary>
    ///     False for a validation dry run: the counts say what an import would do.
    /// </summary>
    public bool Committed { get; init; }

    public int TotalRows { get; init; }
    public int Created { get; init; }
    public int Updated { get; init; }
    public int Skipped { get; init; }
    public int Failed { get; init; }
    public IReadOnlyList<ImportRowResultDto> Rows { get; init; } = [];
}

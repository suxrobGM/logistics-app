using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record ImportRowResultDto
{
    /// <summary>
    ///     1-based row number in the uploaded sheet, header row included, so it matches what the user sees.
    /// </summary>
    public int RowNumber { get; init; }

    public ImportRowAction Action { get; init; }

    /// <summary>
    ///     The natural key the row was matched on (truck number, customer name, load reference...).
    /// </summary>
    public string? Key { get; init; }

    public IReadOnlyList<string> Messages { get; init; } = [];
}

namespace Logistics.Shared.Models;

/// <summary>
///     One spreadsheet column and the field it feeds. A null <see cref="FieldKey"/> leaves the column unused.
/// </summary>
public record ImportColumnMappingDto
{
    public int ColumnIndex { get; init; }
    public string? Header { get; init; }
    public string? FieldKey { get; init; }
}

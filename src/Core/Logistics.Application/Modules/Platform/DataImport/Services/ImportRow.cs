namespace Logistics.Application.Modules.Platform.DataImport.Services;

/// <summary>
///     One data row with its cells keyed by field. <see cref="Headers"/> keeps the source header per
///     field, for parsing that depends on it (a length column labelled "ft").
/// </summary>
internal sealed class ImportRow(
    int rowNumber,
    IReadOnlyDictionary<string, string> values,
    IReadOnlyDictionary<string, string> headers)
{
    public int RowNumber { get; } = rowNumber;

    public IReadOnlyDictionary<string, string> Headers { get; } = headers;

    public string? Get(string fieldKey) => values.GetValueOrDefault(fieldKey);
}

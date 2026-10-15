using Logistics.Application.Abstractions.Documents;
using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Services;

internal static class ImportRowMapper
{
    /// <summary>
    ///     Applies the user's column mapping. Unknown field keys and out-of-range columns are ignored,
    ///     since the mapping comes from the client; a field mapped twice keeps the leftmost column.
    /// </summary>
    public static IReadOnlyList<ImportRow> ToRows(
        SpreadsheetTable table, ImportEntityType entityType, IEnumerable<ImportColumnMappingDto> mapping)
    {
        var knownKeys = ImportFieldCatalog.For(entityType).Select(f => f.Key).ToHashSet();
        var columns = mapping
            .Where(m => m.FieldKey is not null && knownKeys.Contains(m.FieldKey) &&
                        m.ColumnIndex >= 0 && m.ColumnIndex < table.Headers.Count)
            .OrderBy(m => m.ColumnIndex)
            .DistinctBy(m => m.FieldKey)
            .ToList();
        var headers = columns.ToDictionary(c => c.FieldKey!, c => table.Headers[c.ColumnIndex]);

        return
        [
            .. table.Rows.Select(row =>
            {
                var values = new Dictionary<string, string>();
                foreach (var column in columns)
                {
                    if (row.Cells[column.ColumnIndex] is { } value)
                    {
                        values[column.FieldKey!] = value;
                    }
                }

                return new ImportRow(row.RowNumber, values, headers);
            })
        ];
    }

    /// <summary>
    ///     A blank CSV whose header row is each field's first synonym, which auto-maps back to it.
    /// </summary>
    public static string BuildTemplate(ImportEntityType entityType)
    {
        var headers = ImportFieldCatalog.For(entityType).Select(f => f.Synonyms.Count > 0 ? f.Synonyms[0] : f.Label);
        return string.Join(",", headers.Select(Quote)) + "\r\n";
    }

    private static string Quote(string value) =>
        value.IndexOfAny([',', '"', '\n']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}

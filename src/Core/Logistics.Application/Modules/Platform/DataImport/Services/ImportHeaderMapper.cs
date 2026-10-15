using System.Text;
using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Services;

/// <summary>
///     Suggests which field each column feeds: the preset's exact header first, then the synonym
///     table. A field is suggested for one column at most - the leftmost match wins.
/// </summary>
internal static class ImportHeaderMapper
{
    public static IReadOnlyList<ImportColumnMappingDto> Suggest(
        IReadOnlyList<string> headers, ImportEntityType entityType, string? presetId)
    {
        var fields = ImportFieldCatalog.For(entityType);
        var presetHeaders = ImportPresets.Find(presetId)?.Headers.GetValueOrDefault(entityType);
        var preset = presetHeaders?.ToDictionary(p => Normalize(p.Key), p => p.Value) ?? [];
        var synonyms = new Dictionary<string, string>();
        foreach (var field in fields)
        {
            foreach (var synonym in field.Synonyms.Append(field.Label).Append(field.Key))
            {
                synonyms.TryAdd(Normalize(synonym), field.Key);
            }
        }

        var used = new HashSet<string>();
        var mapping = new List<ImportColumnMappingDto>(headers.Count);
        for (var i = 0; i < headers.Count; i++)
        {
            var header = Normalize(headers[i]);
            var key = preset.GetValueOrDefault(header) ?? synonyms.GetValueOrDefault(header);
            if (key is not null && !used.Add(key))
            {
                key = null;
            }

            mapping.Add(new ImportColumnMappingDto { ColumnIndex = i, Header = headers[i], FieldKey = key });
        }

        return mapping;
    }

    /// <summary>
    ///     Lower-case letters and digits only, with <c>#</c> read as "number", so "Unit #", "unit_number"
    ///     and "UNIT NUMBER" compare equal.
    /// </summary>
    public static string Normalize(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return "";
        }

        var sb = new StringBuilder(header.Length + 6);
        foreach (var c in header)
        {
            if (c == '#')
            {
                sb.Append("number");
            }
            else if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }
}

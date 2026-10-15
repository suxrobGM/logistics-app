using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Platform.DataImport.Importers;

/// <summary>
///     A parsed row on its way to becoming a record. Errors make it fail; notices are reported but don't.
/// </summary>
internal abstract class ImportDraft(ImportRow row, string key)
{
    public ImportRow Row { get; } = row;

    /// <summary>
    ///     The natural key, compared case-insensitively against the file and the database.
    /// </summary>
    public string Key { get; } = key;

    public List<string> Errors { get; } = [];
    public List<string> Notices { get; } = [];

    /// <summary>
    ///     Set by the importer's lookup when a record with this key already exists.
    /// </summary>
    public bool Exists { get; set; }

    public ImportRowAction Action { get; set; }
}

using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Platform.DataImport.Commands;

/// <summary>
///     The uploaded spreadsheet shared by preview and import, so both apply the same upload rules.
/// </summary>
public interface IImportFile
{
    Stream Content { get; }
    string FileName { get; }
    string ContentType { get; }
    long FileSizeBytes { get; }
    ImportEntityType EntityType { get; }
}

using Logistics.Application.Abstractions.Documents;
using Logistics.Application.Modules.Platform.DataImport.Commands;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DataImport.Services;

internal static class ImportFileReader
{
    public const int MaxRows = 5000;

    public static Result<SpreadsheetTable> Read(ISpreadsheetReader reader, IImportFile file)
    {
        SpreadsheetTable table;
        try
        {
            table = reader.Read(file.Content, file.FileName);
        }
        catch (InvalidDataException ex)
        {
            return Result<SpreadsheetTable>.Fail(ex.Message);
        }

        if (table.Headers.Count == 0)
        {
            return Result<SpreadsheetTable>.Fail("The file has no header row.");
        }

        return table.Rows.Count > MaxRows
            ? Result<SpreadsheetTable>.Fail(
                $"The file has {table.Rows.Count} rows; split it into files of {MaxRows} or fewer.")
            : Result<SpreadsheetTable>.Ok(table);
    }
}

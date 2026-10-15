namespace Logistics.Application.Abstractions.Documents;

/// <summary>
///     Reads the first sheet of a CSV or XLSX file into strings, so the import code never sees a
///     spreadsheet library type.
/// </summary>
public interface ISpreadsheetReader
{
    /// <summary>
    ///     True when <paramref name="fileName"/> has an extension the reader accepts (.csv, .xlsx).
    /// </summary>
    bool IsSupported(string fileName);

    /// <summary>
    ///     Reads the header row and every non-blank data row. Cells are trimmed; empty cells are
    ///     null. Numbers come back in invariant culture and dates as ISO-8601, whatever the source.
    /// </summary>
    /// <exception cref="InvalidDataException">The file is not a readable CSV or XLSX.</exception>
    SpreadsheetTable Read(Stream content, string fileName);
}

/// <param name="Headers">The first non-blank row, one entry per column.</param>
/// <param name="Rows">Data rows aligned to <paramref name="Headers"/>; RowNumber is the 1-based sheet row.</param>
public sealed record SpreadsheetTable(IReadOnlyList<string> Headers, IReadOnlyList<SpreadsheetRow> Rows);

public sealed record SpreadsheetRow(int RowNumber, IReadOnlyList<string?> Cells);

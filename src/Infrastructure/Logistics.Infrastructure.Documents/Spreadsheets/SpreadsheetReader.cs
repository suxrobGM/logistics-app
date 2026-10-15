using System.Globalization;
using System.Text;
using ExcelDataReader;
using Logistics.Application.Abstractions.Documents;

namespace Logistics.Infrastructure.Documents.Spreadsheets;

public sealed class SpreadsheetReader : ISpreadsheetReader
{
    static SpreadsheetReader()
    {
        // ExcelDataReader resolves legacy code pages while opening workbooks; .NET only ships them
        // through this provider.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static bool IsSupported(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".csv" or ".xlsx";
    }

    public SpreadsheetTable Read(Stream content, string fileName)
    {
        if (!IsSupported(fileName))
        {
            throw new InvalidDataException("Only .csv and .xlsx files are supported.");
        }

        try
        {
            using var reader = Path.GetExtension(fileName).Equals(".csv", StringComparison.OrdinalIgnoreCase)
                ? ExcelReaderFactory.CreateCsvReader(content, new ExcelReaderConfiguration
                {
                    // Web exports are UTF-8 without a BOM far more often than Windows-1252.
                    FallbackEncoding = Encoding.UTF8
                })
                : ExcelReaderFactory.CreateOpenXmlReader(content);

            return ReadTable(reader);
        }
        catch (Exception ex) when (ex is not InvalidDataException)
        {
            throw new InvalidDataException("The file could not be read as a spreadsheet.", ex);
        }
    }

    private static SpreadsheetTable ReadTable(IExcelDataReader reader)
    {
        List<string>? headers = null;
        var rows = new List<SpreadsheetRow>();
        var rowNumber = 0;

        while (reader.Read())
        {
            rowNumber++;
            var cells = new string?[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
            {
                cells[i] = FormatCell(reader.GetValue(i));
            }

            if (cells.All(string.IsNullOrEmpty))
            {
                continue;
            }

            if (headers is null)
            {
                headers = [.. cells.Select((c, i) => c ?? $"Column {i + 1}")];
                continue;
            }

            rows.Add(new SpreadsheetRow(rowNumber, [.. Enumerable.Range(0, headers.Count)
                .Select(i => i < cells.Length ? cells[i] : null)]));
        }

        return new SpreadsheetTable(headers ?? [], rows);
    }

    private static string? FormatCell(object? value)
    {
        var text = value switch
        {
            null or DBNull => null,
            DateTime date when date.TimeOfDay == TimeSpan.Zero => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime date => date.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            double number => number.ToString(CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };

        text = text?.Trim();
        return string.IsNullOrEmpty(text) ? null : text;
    }
}

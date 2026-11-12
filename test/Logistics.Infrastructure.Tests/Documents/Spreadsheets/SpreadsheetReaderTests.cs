using System.IO.Compression;
using System.Text;
using Logistics.Infrastructure.Documents.Spreadsheets;
using Xunit;

namespace Logistics.Infrastructure.Documents.Tests.Spreadsheets;

public class SpreadsheetReaderTests
{
    private readonly SpreadsheetReader _sut = new();

    #region CSV

    [Fact]
    public void Read_CsvWithBomAndQuotedCommas_KeepsQuotedFieldsWhole()
    {
        var csv = "Unit #,Notes,Rate\r\n101,\"Hooked, ready\",\"1,250.00\"\r\n102,\"Said \"\"hi\"\"\",900\r\n";
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

        var table = _sut.Read(new MemoryStream(bytes), "trucks.csv");

        Assert.Equal(["Unit #", "Notes", "Rate"], table.Headers);
        Assert.Equal(2, table.Rows.Count);
        Assert.Equal("Hooked, ready", table.Rows[0].Cells[1]);
        Assert.Equal("1,250.00", table.Rows[0].Cells[2]);
        Assert.Equal("Said \"hi\"", table.Rows[1].Cells[1]);
    }

    [Fact]
    public void Read_CsvWithBlankLinesAndEmptyCells_SkipsBlankRowsAndNullsEmptyCells()
    {
        var csv = "Name,Email\n\nAcme Freight,\n,\nBeta LLC,ops@beta.test\n";

        var table = _sut.Read(new MemoryStream(Encoding.UTF8.GetBytes(csv)), "customers.csv");

        Assert.Equal(2, table.Rows.Count);
        Assert.Null(table.Rows[0].Cells[1]);
        Assert.Equal(5, table.Rows[1].RowNumber);
    }

    [Fact]
    public void Read_UnsupportedExtension_Throws()
    {
        Assert.Throws<InvalidDataException>(() => _sut.Read(new MemoryStream([1, 2, 3]), "data.xls"));
    }

    #endregion

    #region XLSX

    [Fact]
    public void Read_Xlsx_ReadsStringsNumbersAndDates()
    {
        var table = _sut.Read(BuildXlsx(), "loads.xlsx");

        Assert.Equal(["Load #", "Rate", "Pickup Date"], table.Headers);
        var row = Assert.Single(table.Rows);
        Assert.Equal("L-1", row.Cells[0]);
        Assert.Equal("1250.5", row.Cells[1]);
        Assert.Equal("2026-03-04", row.Cells[2]);
    }

    [Fact]
    public void Read_CorruptXlsx_ThrowsInvalidData()
    {
        Assert.Throws<InvalidDataException>(() =>
            _sut.Read(new MemoryStream(Encoding.UTF8.GetBytes("not a zip")), "broken.xlsx"));
    }

    /// <summary>
    ///     The smallest workbook Excel itself would open: inline strings, a number and a date serial
    ///     (46085 = 2026-03-04) styled with the built-in date format 14.
    /// </summary>
    private static MemoryStream BuildXlsx()
    {
        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            Add(zip, "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                </Types>
                """);
            Add(zip, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            Add(zip, "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="Loads" sheetId="1" r:id="rId1"/></sheets>
                </workbook>
                """);
            Add(zip, "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """);
            Add(zip, "xl/styles.xml",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <cellXfs count="2"><xf numFmtId="0"/><xf numFmtId="14" applyNumberFormat="1"/></cellXfs>
                </styleSheet>
                """);
            Add(zip, "xl/worksheets/sheet1.xml",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <sheetData>
                    <row r="1">
                      <c r="A1" t="inlineStr"><is><t>Load #</t></is></c>
                      <c r="B1" t="inlineStr"><is><t>Rate</t></is></c>
                      <c r="C1" t="inlineStr"><is><t>Pickup Date</t></is></c>
                    </row>
                    <row r="2">
                      <c r="A2" t="inlineStr"><is><t>L-1</t></is></c>
                      <c r="B2"><v>1250.5</v></c>
                      <c r="C2" s="1"><v>46085</v></c>
                    </row>
                  </sheetData>
                </worksheet>
                """);
        }

        stream.Position = 0;
        return stream;
    }

    private static void Add(ZipArchive zip, string path, string content)
    {
        using var writer = new StreamWriter(zip.CreateEntry(path).Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    #endregion
}

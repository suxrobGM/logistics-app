using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

namespace Logistics.Infrastructure.Services.Pdf.Shared;

/// <summary>
/// Reusable QuestPDF cell-style helpers shared by all invoice/pay-stub PDFs.
/// </summary>
internal static class PdfStyles
{
    public static IContainer HeaderCell(IContainer container) =>
        container.Background(Colors.Blue.Darken3)
                 .Padding(8)
                 .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(10));

    public static IContainer EarningsHeaderCell(IContainer container) =>
        container.Background(Colors.Green.Darken2)
                 .Padding(8)
                 .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(10));

    public static IContainer DeductionsHeaderCell(IContainer container) =>
        container.Background(Colors.Red.Darken2)
                 .Padding(8)
                 .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(10));

    public static IContainer Cell(IContainer container) =>
        container.BorderBottom(1)
                 .BorderColor(Colors.Grey.Lighten2)
                 .Padding(8)
                 .DefaultTextStyle(x => x.FontSize(10));

    public static IContainer SmallHeaderCell(IContainer container) =>
        container.Background(Colors.Grey.Lighten3)
                 .Padding(5)
                 .DefaultTextStyle(x => x.Bold().FontSize(9));

    public static IContainer SmallCell(IContainer container) =>
        container.BorderBottom(1)
                 .BorderColor(Colors.Grey.Lighten3)
                 .Padding(5)
                 .DefaultTextStyle(x => x.FontSize(9));
}

using System.Globalization;
using Logistics.Application.Abstractions.Documents;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Documents.Pdf.Shared;
using Logistics.Infrastructure.Documents.Pdf.VehicleBol;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestDocument = QuestPDF.Fluent.Document;

namespace Logistics.Infrastructure.Documents.Pdf;

public sealed class VehicleBolPdfService : IVehicleBolPdfService
{
    private const float DiagramWidth = 110;

    public byte[] Generate(VehicleBolDocument document)
    {
        var pdf = QuestDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => RenderHeader(c, document));

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(12);
                    col.Item().Element(c => RenderRoute(c, document));

                    foreach (var vehicle in document.Vehicles)
                    {
                        col.Item().ShowEntire().Element(c => RenderVehicle(c, vehicle, document.Currency));
                    }

                    col.Item().Element(RenderLegend);
                    col.Item().ShowEntire().Element(c => RenderSignatures(c, document));
                });

                page.Footer().Element(c => PdfFooter.Render(c, document.Carrier,
                    $"Electronic bill of lading generated {FormatTime(document.GeneratedAt)}"));
            });
        });

        return pdf.GeneratePdf();
    }

    private static void RenderHeader(IContainer container, VehicleBolDocument document)
    {
        container.Row(row =>
        {
            row.RelativeItem().Element(c => CompanyInfoColumn.Render(c, document.Carrier, includeTaxIds: true));
            row.ConstantItem(200).AlignRight().Column(col =>
            {
                col.Item().AlignRight().Text("VEHICLE BILL OF LADING").FontSize(14).Bold();
                col.Item().AlignRight().Text($"Load #{document.LoadNumber}").FontSize(11);
                col.Item().AlignRight().Text(document.LoadName);
                if (!string.IsNullOrEmpty(document.TruckNumber))
                {
                    col.Item().AlignRight().Text($"Truck: {document.TruckNumber}");
                }

                if (!string.IsNullOrEmpty(document.DriverName))
                {
                    col.Item().AlignRight().Text($"Driver: {document.DriverName}");
                }
            });
        });
    }

    private static void RenderRoute(IContainer container, VehicleBolDocument document)
    {
        container.Row(row =>
        {
            row.Spacing(12);
            row.RelativeItem().Element(c => RenderStop(c, "Pickup", document.Origin.ToString(), document.Pickup));
            row.RelativeItem().Element(c => RenderStop(c, "Delivery", document.Destination.ToString(), document.Delivery));
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
            {
                col.Item().Text("Customer").Bold();
                col.Item().Text(document.CustomerName);
                col.Item().PaddingTop(4).Text($"Vehicles: {document.Vehicles.Count}");
            });
        });
    }

    private static void RenderStop(IContainer container, string title, string address, VehicleBolSignature? signature)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
        {
            col.Item().Text(title).Bold();
            col.Item().Text(address);
            if (signature?.SignedAt is { } signedAt)
            {
                col.Item().PaddingTop(4).Text(FormatTime(signedAt)).FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private static void RenderVehicle(IContainer container, VehicleBolVehicle item, string currency)
    {
        var vehicle = item.Vehicle;

        container.Border(1).BorderColor(Colors.Grey.Lighten1).Column(col =>
        {
            col.Item().Background(Colors.Grey.Lighten4).Padding(6).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span(vehicle.GetDisplayName()).Bold();
                    if (!string.IsNullOrEmpty(vehicle.Color))
                    {
                        text.Span($" - {vehicle.Color}");
                    }
                });
                row.AutoItem().Text($"VIN: {vehicle.Vin ?? "n/a"}");
            });

            col.Item().PaddingHorizontal(6).PaddingTop(4).Text(text =>
            {
                text.Span(vehicle.IsOperable ? "Operable" : "INOPERABLE").Bold();
                if (!string.IsNullOrEmpty(vehicle.LotNumber))
                {
                    text.Span($"   Lot/Stock: {vehicle.LotNumber}");
                }

                if (vehicle.Rate is { } rate)
                {
                    text.Span($"   Rate: {rate.ToString("N2", CultureInfo.InvariantCulture)} {currency}");
                }

                if (!string.IsNullOrEmpty(vehicle.Notes))
                {
                    text.Span($"   Notes: {vehicle.Notes}");
                }
            });

            col.Item().Padding(6).Row(row =>
            {
                row.Spacing(10);
                row.RelativeItem().Element(c => RenderInspection(c, "At pickup", item.Pickup));
                row.RelativeItem().Element(c => RenderInspection(c, "At delivery", item.Delivery));
            });
        });
    }

    private static void RenderInspection(IContainer container, string title, VehicleBolInspection? inspection)
    {
        container.Column(col =>
        {
            col.Item().Text(title).Bold();

            if (inspection is null)
            {
                col.Item().PaddingTop(4).Text("Not inspected").FontColor(Colors.Grey.Darken1);
                return;
            }

            col.Item().Text(InspectionLine(inspection)).FontSize(8).FontColor(Colors.Grey.Darken1);

            col.Item().PaddingTop(4).Row(row =>
            {
                row.ConstantItem(DiagramWidth).Svg(VehicleDiagram.BuildSvg(inspection.Defects));
                row.RelativeItem().PaddingLeft(6).Column(defects => RenderDefectList(defects, inspection));
            });
        });
    }

    private static void RenderDefectList(ColumnDescriptor col, VehicleBolInspection inspection)
    {
        if (inspection.Defects.Count == 0)
        {
            col.Item().Text("No damage noted");
        }

        foreach (var defect in inspection.Defects)
        {
            col.Item().Text(DefectLine(defect)).FontSize(8);
        }

        if (!string.IsNullOrEmpty(inspection.Notes))
        {
            col.Item().PaddingTop(4).Text($"Notes: {inspection.Notes}").FontSize(8);
        }
    }

    private static void RenderLegend(IContainer container)
    {
        var codes = Enum.GetValues<VehicleDamageType>().Select(t => $"{t.GetCode()} = {t.GetDescription()}");
        container.Text($"Damage codes: {string.Join("   ", codes)}").FontSize(8).FontColor(Colors.Grey.Darken2);
    }

    private static void RenderSignatures(IContainer container, VehicleBolDocument document)
    {
        container.Row(row =>
        {
            row.Spacing(12);
            row.RelativeItem().Element(c => RenderSignature(c, "Shipper (pickup)", document.Pickup));
            row.RelativeItem().Element(c => RenderSignature(c, "Receiver (delivery)", document.Delivery));
        });
    }

    private static void RenderSignature(IContainer container, string title, VehicleBolSignature? signature)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
        {
            col.Item().Text(title).Bold();

            if (signature is null)
            {
                col.Item().PaddingTop(4).Text("Not yet signed").FontColor(Colors.Grey.Darken1);
                return;
            }

            if (signature.SignatureImage is { Length: > 0 } image)
            {
                col.Item().Height(50).AlignLeft().Image(image).FitArea();
            }
            else if (!string.IsNullOrEmpty(signature.UnableToSignReason))
            {
                col.Item().PaddingTop(4).Text($"Unable to sign: {signature.UnableToSignReason}");
            }

            col.Item().Text($"Name: {signature.SignerName ?? "n/a"}");
            if (signature.SignedAt is { } signedAt)
            {
                col.Item().Text($"Signed: {FormatTime(signedAt)}");
            }

            if (FormatGps(signature.Latitude, signature.Longitude) is { } gps)
            {
                col.Item().Text($"GPS: {gps}");
            }
        });
    }

    private static string InspectionLine(VehicleBolInspection inspection)
    {
        var parts = new List<string> { FormatTime(inspection.InspectedAt) };
        if (!string.IsNullOrEmpty(inspection.InspectorName))
        {
            parts.Add(inspection.InspectorName);
        }

        if (FormatGps(inspection.Latitude, inspection.Longitude) is { } gps)
        {
            parts.Add(gps);
        }

        return string.Join(" | ", parts);
    }

    private static string DefectLine(ConditionDefect defect)
    {
        var where = defect.Zone?.GetDescription() ?? defect.PartCategory.GetDescription();
        var code = defect.DamageType is { } type ? $"{type.GetCode()} " : string.Empty;
        var description = string.IsNullOrEmpty(defect.Description) ? string.Empty : $" - {defect.Description}";
        return $"{code}{where} ({defect.Severity}){description}";
    }

    private static string? FormatGps(double? latitude, double? longitude) =>
        latitude is { } lat && longitude is { } lng
            ? string.Create(CultureInfo.InvariantCulture, $"{lat:F5}, {lng:F5}")
            : null;

    private static string FormatTime(DateTime value) =>
        value.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);
}

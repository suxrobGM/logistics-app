using Logistics.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Services.Pdf.Shared;

/// <summary>
/// Renders the tenant's letterhead block (name, address, phone, billing email, regulatory IDs).
/// Set <paramref name="includeTaxIds"/> to surface VAT / EORI / MC numbers — useful on invoices,
/// noise on internal pay stubs.
/// </summary>
internal static class CompanyInfoColumn
{
    public static void Render(IContainer container, Tenant tenant, bool includeTaxIds)
    {
        container.Column(col =>
        {
            col.Item().Text(tenant.CompanyName ?? tenant.Name)
                .FontSize(18)
                .Bold()
                .FontColor(Colors.Blue.Darken3);

            col.Item().Text(tenant.CompanyAddress.Line1);
            if (!string.IsNullOrEmpty(tenant.CompanyAddress.Line2))
            {
                col.Item().Text(tenant.CompanyAddress.Line2);
            }
            col.Item().Text(PdfFormatting.CityLine(tenant.CompanyAddress));

            if (!string.IsNullOrEmpty(tenant.PhoneNumber))
            {
                col.Item().Text($"Phone: {tenant.PhoneNumber}");
            }
            col.Item().Text($"Email: {tenant.BillingEmail}");

            if (!string.IsNullOrEmpty(tenant.DotNumber))
            {
                col.Item().Text($"DOT#: {tenant.DotNumber}");
            }

            if (!includeTaxIds) return;

            if (!string.IsNullOrEmpty(tenant.McNumber))   col.Item().Text($"MC#: {tenant.McNumber}");
            if (!string.IsNullOrEmpty(tenant.VatNumber))  col.Item().Text($"VAT: {tenant.VatNumber}");
            if (!string.IsNullOrEmpty(tenant.EoriNumber)) col.Item().Text($"EORI: {tenant.EoriNumber}");
        });
    }
}

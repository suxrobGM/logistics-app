using Logistics.Domain.Entities;
using Logistics.Infrastructure.Services.Pdf.Payroll;
using Logistics.Infrastructure.Services.Pdf.Shared;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Logistics.Application.Abstractions.Documents;
using QuestDocument = QuestPDF.Fluent.Document;

namespace Logistics.Infrastructure.Services.Pdf;

public class PayrollPayStubService : IPayrollPayStubService
{
    public byte[] GeneratePayStubPdf(PayrollInvoice payroll, Tenant tenant)
    {
        Settings.License = LicenseType.Community;

        var doc = QuestDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => PayrollHeaderSection.Render(c, payroll, tenant));
                page.Content().PaddingVertical(20).Element(c => PayrollEarningsSection.Render(c, payroll));
                page.Footer().Element(c => PdfFooter.Render(c, tenant, "This is a pay stub for your records."));
            });
        });

        return doc.GeneratePdf();
    }
}

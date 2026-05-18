using Logistics.Application.Abstractions.Privacy;
using Logistics.Infrastructure.Documents.Privacy;
using Logistics.Infrastructure.Services.Pdf;
using Logistics.Infrastructure.Services.PdfImport;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.Documents;

namespace Logistics.Infrastructure.Documents;

public static class Registrar
{
    /// <summary>
    ///     Add document infrastructure (PDF generation/import, GDPR data export).
    /// </summary>
    public static IServiceCollection AddDocumentsInfrastructure(this IServiceCollection services)
    {
        // PDF generation
        services.AddScoped<IInvoicePdfService, InvoicePdfService>();
        services.AddScoped<IPayrollPayStubService, PayrollPayStubService>();

        // PDF import (template-based extraction)
        services.AddScoped<IPdfDataExtractor, TemplateBasedDataExtractor>();

        // Privacy / GDPR
        services.AddScoped<IDataExportService, DataExportService>();
        return services;
    }
}

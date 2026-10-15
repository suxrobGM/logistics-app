using Logistics.Application.Abstractions.Privacy;
using Logistics.Infrastructure.Documents.Privacy;
using Logistics.Infrastructure.Documents.Pdf;
using Logistics.Infrastructure.Documents.PdfImport;
using Logistics.Infrastructure.Documents.Spreadsheets;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.Documents;
using QuestPDF;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Documents;

public static class Registrar
{
    /// <summary>
    ///     Add document infrastructure (PDF generation/import, spreadsheet import, GDPR data export).
    /// </summary>
    public static IServiceCollection AddDocumentsInfrastructure(this IServiceCollection services)
    {
        Settings.License = LicenseType.Community;

        // PDF generation
        services.AddScoped<IInvoicePdfService, InvoicePdfService>();
        services.AddScoped<IPayrollPayStubService, PayrollPayStubService>();
        services.AddScoped<IIftaReportPdfService, Pdf.Ifta.IftaReportPdfService>();
        services.AddScoped<IVehicleBolPdfService, VehicleBolPdfService>();

        // PDF import (LLM-based extraction)
        services.AddScoped<IPdfDataExtractor, LlmPdfDataExtractor>();

        // CSV / XLSX data import
        services.AddSingleton<ISpreadsheetReader, SpreadsheetReader>();

        // Privacy / GDPR
        services.AddScoped<IDataExportService, DataExportService>();
        return services;
    }
}

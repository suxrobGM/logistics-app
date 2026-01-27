using Logistics.Application.Services;
using Logistics.Application.Services.Pdf;
using Logistics.Application.Services.PdfImport;
using Logistics.Infrastructure.Documents.Vin;
using Logistics.Infrastructure.Services.Pdf;
using Logistics.Infrastructure.Services.PdfImport;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Documents;

public static class Registrar
{
    /// <summary>
    ///     Add document infrastructure (PDF generation/import, VIN decoder).
    /// </summary>
    public static IServiceCollection AddDocumentsInfrastructure(this IServiceCollection services)
    {
        // PDF generation
        services.AddScoped<IInvoicePdfService, InvoicePdfService>();
        services.AddScoped<IPayrollPayStubService, PayrollPayStubService>();

        // PDF import (template-based extraction)
        services.AddScoped<IPdfDataExtractor, TemplateBasedDataExtractor>();

        // VIN decoder
        services.AddHttpClient<IVinDecoderService, NhtsaVinDecoderService>();
        return services;
    }
}

using Logistics.Domain.Entities;

namespace Logistics.Application.Services.Pdf;

/// <summary>
/// Service for generating invoice PDF documents.
/// </summary>
public interface IInvoicePdfService
{
    /// <summary>
    /// Generates a PDF for a load invoice.
    /// </summary>
    /// <param name="invoice">The invoice entity with related data loaded.</param>
    /// <param name="tenant">The tenant for company information.</param>
    /// <returns>PDF file as byte array.</returns>
    byte[] GenerateLoadInvoicePdf(LoadInvoice invoice, Tenant tenant);
}

using Logistics.Domain.Entities;

namespace Logistics.Application.Services.Pdf;

/// <summary>
/// Service for generating payroll pay stub PDF documents.
/// </summary>
public interface IPayrollPayStubService
{
    /// <summary>
    /// Generates a PDF pay stub for a payroll invoice.
    /// </summary>
    /// <param name="payroll">The payroll invoice entity with related data loaded.</param>
    /// <param name="tenant">The tenant for company information.</param>
    /// <returns>PDF file as byte array.</returns>
    byte[] GeneratePayStubPdf(PayrollInvoice payroll, Tenant tenant);
}

using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal static class InvoiceFactoring
{
    /// <summary>
    /// The invoice's factoring status, or null when neither the customer nor the invoice involves a
    /// factor - so unfactored invoices carry no extra key the model has to reason about.
    /// </summary>
    public static string? GetStatus(InvoiceDto invoice) =>
        invoice.Customer?.FactoringCompanyId is not null || invoice.FactoringStatus != FactoringStatus.NotFactored
            ? invoice.FactoringStatus.ToString()
            : null;
}

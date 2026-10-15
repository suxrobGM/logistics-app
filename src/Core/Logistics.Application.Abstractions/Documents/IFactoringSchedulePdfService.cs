using Logistics.Domain.Entities;

namespace Logistics.Application.Abstractions.Documents;

/// <summary>
/// Renders the schedule of accounts a carrier sends its factoring company with a batch of invoices.
/// </summary>
public interface IFactoringSchedulePdfService
{
    /// <param name="batch">The schedule; its items' invoices (with load and customer) are read.</param>
    /// <param name="tenant">The carrier, printed as the factor's client.</param>
    byte[] GenerateSchedulePdf(FactoringBatch batch, Tenant tenant);
}

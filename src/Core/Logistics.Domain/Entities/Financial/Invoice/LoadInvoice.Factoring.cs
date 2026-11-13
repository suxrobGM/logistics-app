using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public partial class LoadInvoice
{
    /// <summary>
    /// Where the invoice stands with the factoring company; the history is on <see cref="FactoringBatchItem"/>.
    /// </summary>
    public FactoringStatus FactoringStatus { get; set; } = FactoringStatus.NotFactored;

    /// <summary>
    /// The factor the customer pays under its notice of assignment, which covers every invoice to that
    /// customer whatever its <see cref="FactoringStatus"/>; null when the carrier collects.
    /// </summary>
    public FactoringCompany? AssignedFactor => Customer?.FactoringCompany;
}

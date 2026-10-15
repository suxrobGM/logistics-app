using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public partial class LoadInvoice
{
    /// <summary>
    /// Where the invoice stands with the factoring company; the history is on <see cref="FactoringBatchItem"/>.
    /// </summary>
    public FactoringStatus FactoringStatus { get; set; } = FactoringStatus.NotFactored;
}

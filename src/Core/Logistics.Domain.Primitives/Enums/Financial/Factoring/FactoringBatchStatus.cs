namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// State of a schedule of accounts sent to a factoring company, derived from its invoices.
/// </summary>
public enum FactoringBatchStatus
{
    Submitted,
    PartiallyFunded,
    Funded,
    Rejected
}

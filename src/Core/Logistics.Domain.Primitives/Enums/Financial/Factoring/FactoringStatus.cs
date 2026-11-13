namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Where a load invoice stands with the factoring company.
/// </summary>
public enum FactoringStatus
{
    NotFactored,
    Submitted,
    Funded,
    Rejected,
    ChargedBack
}

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Who carries the loss when a factored invoice goes unpaid. Under recourse the factor charges the
/// invoice back to the carrier; under non-recourse the factor absorbs a customer's insolvency.
/// </summary>
public enum FactoringRecourseType
{
    Recourse,
    NonRecourse
}

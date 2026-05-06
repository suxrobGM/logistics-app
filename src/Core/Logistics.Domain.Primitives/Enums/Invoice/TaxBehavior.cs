namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// How tax is applied to an invoice's line item amounts.
/// </summary>
public enum TaxBehavior
{
    /// <summary>
    /// Line item amounts are net (pre-tax). Tax is added on top: Total = Subtotal + TaxTotal.
    /// </summary>
    Exclusive = 1,

    /// <summary>
    /// Line item amounts are gross (tax-inclusive). Subtotal is derived by removing the embedded tax.
    /// </summary>
    Inclusive = 2,

    /// <summary>
    /// EU cross-border B2B: no VAT charged; recipient self-accounts. TaxTotal == 0.
    /// </summary>
    ReverseCharge = 3
}

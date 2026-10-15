namespace Logistics.Domain.Primitives.Enums;

public static class InvoiceLineItemTypeExtensions
{
    /// <summary>
    /// Settlement line types that reduce driver pay. Their amounts are stored negative, so a
    /// payroll invoice's total is the net pay.
    /// </summary>
    public static bool IsPayDeduction(this InvoiceLineItemType type) =>
        type is InvoiceLineItemType.Deduction
            or InvoiceLineItemType.Escrow
            or InvoiceLineItemType.CashAdvance
            or InvoiceLineItemType.FuelCard;
}

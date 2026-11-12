namespace Logistics.Domain.Primitives.Enums;

public static class InvoiceLineItemTypeExtensions
{
    extension(InvoiceLineItemType type)
    {
        /// <summary>
        /// Settlement line types that reduce driver pay. Their amounts are stored negative, so a
        /// payroll invoice's total is the net pay.
        /// </summary>
        public bool IsPayDeduction() =>
            type is InvoiceLineItemType.Deduction
                or InvoiceLineItemType.Escrow
                or InvoiceLineItemType.CashAdvance
                or InvoiceLineItemType.FuelCard;
    }
}

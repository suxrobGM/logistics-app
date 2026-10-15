namespace Logistics.Domain.Primitives.Enums;

public enum InvoiceLineItemType
{
    BaseRate,
    FuelSurcharge,
    Detention,
    Layover,
    Lumper,
    Accessorial,
    Demurrage,
    PerDiem,
    Chassis,
    Storage,
    Discount,
    Tax,
    Other,

    // Payroll-specific line item types
    BasePay,
    Bonus,
    Deduction,
    Reimbursement,
    Adjustment
}

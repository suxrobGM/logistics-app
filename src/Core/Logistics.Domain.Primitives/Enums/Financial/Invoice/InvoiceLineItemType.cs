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
    Adjustment,

    /// <summary>Settlement deduction held in the driver's escrow balance; a positive line releases escrow.</summary>
    Escrow,

    /// <summary>Settlement deduction repaying a cash advance.</summary>
    CashAdvance,

    /// <summary>Settlement deduction for fuel card purchases on the driver's truck.</summary>
    FuelCard
}

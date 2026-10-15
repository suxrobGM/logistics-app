namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// The local domain entity type that a QuickBooks sync mapping refers to.
/// </summary>
public enum QboLocalEntityType
{
    Customer = 1,
    Invoice = 2,
    Payment = 3,
    Expense = 4,

    /// <summary>An approved payroll invoice, pushed as a QBO Bill.</summary>
    PayrollInvoice = 5,

    /// <summary>An employee, pushed as the QBO Vendor their payroll bills are owed to.</summary>
    Employee = 6,

    /// <summary>The fee a factoring company kept on a funded invoice, pushed as a QBO Purchase from the clearing account.</summary>
    FactoringFee = 7
}

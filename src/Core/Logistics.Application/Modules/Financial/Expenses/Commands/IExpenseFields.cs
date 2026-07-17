namespace Logistics.Application.Modules.Financial.Expenses.Commands;

/// <summary>
/// The expense fields shared by the create and update commands, so all of them inherit one set of
/// validation rules.
/// </summary>
public interface IExpenseFields
{
    decimal Amount { get; }
    string Currency { get; }
    string? VendorName { get; }
    DateTime ExpenseDate { get; }
    string? ReceiptBlobPath { get; }
    string? Notes { get; }
}

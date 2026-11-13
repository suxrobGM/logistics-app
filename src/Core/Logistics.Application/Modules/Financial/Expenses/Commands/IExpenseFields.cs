namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public interface IExpenseFields
{
    decimal Amount { get; }
    string? VendorName { get; }
    DateOnly ExpenseDate { get; }
    string? ReceiptBlobPath { get; }
    string? Notes { get; }
}

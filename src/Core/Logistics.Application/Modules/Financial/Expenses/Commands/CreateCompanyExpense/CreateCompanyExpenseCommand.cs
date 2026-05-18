using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class CreateCompanyExpenseCommand : ICommand<Result<Guid>>
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? VendorName { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? ReceiptBlobPath { get; set; }
    public string? Notes { get; set; }
    public CompanyExpenseCategory Category { get; set; }
}

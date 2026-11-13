using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
///     General company expenses (office supplies, software, insurance, legal, travel, etc.).
/// </summary>
public class CompanyExpense : Expense
{
    public override ExpenseType Type { get; set; } = ExpenseType.Company;

    /// <summary>
    ///     Category of the company expense.
    /// </summary>
    public CompanyExpenseCategory Category { get; set; }
}

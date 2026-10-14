using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives.ValueObjects;

public record CompanyExpenseCategoryMapping
{
    public required CompanyExpenseCategory Category { get; set; }
    public required string QboId { get; set; }
}

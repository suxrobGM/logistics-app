using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Primitives.ValueObjects;

public record TruckExpenseCategoryMapping
{
    public required TruckExpenseCategory Category { get; set; }
    public required string QboId { get; set; }
}

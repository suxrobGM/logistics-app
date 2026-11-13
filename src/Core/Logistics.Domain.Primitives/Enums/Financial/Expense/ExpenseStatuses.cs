namespace Logistics.Domain.Primitives.Enums;

/// <summary>Pending expenses may still be rejected, so only these count as costs. An array so EF can translate <c>Contains</c>.</summary>
public static class ExpenseStatuses
{
    public static readonly ExpenseStatus[] Counted = [ExpenseStatus.Approved, ExpenseStatus.Paid];
}

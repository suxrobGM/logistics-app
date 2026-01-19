using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record ExpenseDto
{
    public Guid Id { get; init; }
    public long Number { get; init; }
    public ExpenseType Type { get; init; }
    public ExpenseStatus Status { get; init; }

    /// <summary>
    ///     Total expense amount.
    /// </summary>
    public Money Amount { get; init; } = null!;

    public string? VendorName { get; init; }
    public DateTime ExpenseDate { get; init; }
    public string ReceiptBlobPath { get; init; } = string.Empty;
    public string? ReceiptDownloadUrl { get; init; }
    public string? Notes { get; init; }

    // Approval info
    public string? ApprovedById { get; init; }
    public string? ApprovedByName { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string? RejectionReason { get; init; }

    // Audit fields
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string? CreatedByName { get; init; }

    // CompanyExpense fields
    public CompanyExpenseCategory? CompanyCategory { get; init; }

    // TruckExpense fields
    public Guid? TruckId { get; init; }
    public TruckDto? Truck { get; init; }
    public TruckExpenseCategory? TruckCategory { get; init; }
    public int? OdometerReading { get; init; }

    // BodyShopExpense fields
    public string? VendorAddress { get; init; }
    public string? VendorPhone { get; init; }
    public string? RepairDescription { get; init; }
    public DateTime? EstimatedCompletionDate { get; init; }
    public DateTime? ActualCompletionDate { get; init; }
}

using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class UpdateExpenseCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? VendorName { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? ReceiptBlobPath { get; set; }
    public string? Notes { get; set; }

    // CompanyExpense fields
    public CompanyExpenseCategory? CompanyCategory { get; set; }

    // TruckExpense fields
    public TruckExpenseCategory? TruckCategory { get; set; }
    public int? OdometerReading { get; set; }

    // BodyShopExpense fields
    public string? VendorAddress { get; set; }
    public string? VendorPhone { get; set; }
    public string? RepairDescription { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
}

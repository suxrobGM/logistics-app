using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class CreateTruckExpenseCommand : ICommand<Result<Guid>>
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? VendorName { get; set; }
    public DateTime ExpenseDate { get; set; }
    public required string ReceiptBlobPath { get; set; }
    public string? Notes { get; set; }
    public required Guid TruckId { get; set; }
    public TruckExpenseCategory Category { get; set; }
    public int? OdometerReading { get; set; }
    public decimal? Quantity { get; set; }
    public VolumeUnit? QuantityUnit { get; set; }
}

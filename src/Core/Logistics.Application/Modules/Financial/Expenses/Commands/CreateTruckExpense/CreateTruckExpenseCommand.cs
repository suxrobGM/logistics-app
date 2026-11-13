using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class CreateTruckExpenseCommand : ICommand<Result<Guid>>, IExpenseFields
{
    public decimal Amount { get; set; }
    public string? VendorName { get; set; }
    public DateTime ExpenseDate { get; set; }
    public required string ReceiptBlobPath { get; set; }
    public string? Notes { get; set; }
    public required Guid TruckId { get; set; }
    public TruckExpenseCategory Category { get; set; }
    public long? OdometerMeters { get; set; }
    public decimal? Quantity { get; set; }
    public VolumeUnit? QuantityUnit { get; set; }
}

using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class CreateBodyShopExpenseCommand : ICommand<Result<Guid>>, IExpenseFields
{
    public decimal Amount { get; set; }
    public string? VendorName { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public required string ReceiptBlobPath { get; set; }
    public string? Notes { get; set; }
    public required Guid TruckId { get; set; }
    public string? VendorAddress { get; set; }
    public string? VendorPhone { get; set; }
    public string? RepairDescription { get; set; }
    public DateOnly? EstimatedCompletionDate { get; set; }
    public DateOnly? ActualCompletionDate { get; set; }
}

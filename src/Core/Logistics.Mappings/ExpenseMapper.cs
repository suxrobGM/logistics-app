using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class ExpenseMapper
{
    public static ExpenseDto ToDto(this Expense entity, string? receiptDownloadUrl = null)
    {
        return entity switch
        {
            CompanyExpense companyExpense => new ExpenseDto
            {
                Id = companyExpense.Id,
                Number = companyExpense.Number,
                Type = ExpenseType.Company,
                Status = companyExpense.Status,
                Amount = companyExpense.Amount,
                VendorName = companyExpense.VendorName,
                ExpenseDate = companyExpense.ExpenseDate,
                ReceiptBlobPath = companyExpense.ReceiptBlobPath,
                ReceiptDownloadUrl = receiptDownloadUrl,
                Notes = companyExpense.Notes,
                ApprovedById = companyExpense.ApprovedById,
                ApprovedAt = companyExpense.ApprovedAt,
                RejectionReason = companyExpense.RejectionReason,
                CreatedAt = companyExpense.CreatedAt,
                CreatedBy = companyExpense.CreatedBy,
                CompanyCategory = companyExpense.Category
            },
            TruckExpense truckExpense => new ExpenseDto
            {
                Id = truckExpense.Id,
                Number = truckExpense.Number,
                Type = ExpenseType.Truck,
                Status = truckExpense.Status,
                Amount = truckExpense.Amount,
                VendorName = truckExpense.VendorName,
                ExpenseDate = truckExpense.ExpenseDate,
                ReceiptBlobPath = truckExpense.ReceiptBlobPath,
                ReceiptDownloadUrl = receiptDownloadUrl,
                Notes = truckExpense.Notes,
                ApprovedById = truckExpense.ApprovedById,
                ApprovedAt = truckExpense.ApprovedAt,
                RejectionReason = truckExpense.RejectionReason,
                CreatedAt = truckExpense.CreatedAt,
                CreatedBy = truckExpense.CreatedBy,
                TruckId = truckExpense.TruckId,
                Truck = truckExpense.Truck.ToDto(),
                TruckCategory = truckExpense.Category,
                OdometerReading = truckExpense.OdometerReading
            },
            BodyShopExpense bodyShopExpense => new ExpenseDto
            {
                Id = bodyShopExpense.Id,
                Number = bodyShopExpense.Number,
                Type = ExpenseType.BodyShop,
                Status = bodyShopExpense.Status,
                Amount = bodyShopExpense.Amount,
                VendorName = bodyShopExpense.VendorName,
                ExpenseDate = bodyShopExpense.ExpenseDate,
                ReceiptBlobPath = bodyShopExpense.ReceiptBlobPath,
                ReceiptDownloadUrl = receiptDownloadUrl,
                Notes = bodyShopExpense.Notes,
                ApprovedById = bodyShopExpense.ApprovedById,
                ApprovedAt = bodyShopExpense.ApprovedAt,
                RejectionReason = bodyShopExpense.RejectionReason,
                CreatedAt = bodyShopExpense.CreatedAt,
                CreatedBy = bodyShopExpense.CreatedBy,
                TruckId = bodyShopExpense.TruckId,
                Truck = bodyShopExpense.Truck.ToDto(),
                VendorAddress = bodyShopExpense.VendorAddress,
                VendorPhone = bodyShopExpense.VendorPhone,
                RepairDescription = bodyShopExpense.RepairDescription,
                EstimatedCompletionDate = bodyShopExpense.EstimatedCompletionDate,
                ActualCompletionDate = bodyShopExpense.ActualCompletionDate
            },
            _ => throw new NotImplementedException($"Mapping for {entity.GetType().Name} is not implemented.")
        };
    }
}

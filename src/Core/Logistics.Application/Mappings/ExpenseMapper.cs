using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class ExpenseMapper
{
    [MapperIgnoreTarget(nameof(ExpenseDto.Type))]
    [MapperIgnoreTarget(nameof(ExpenseDto.ReceiptDownloadUrl))]
    [MapperIgnoreTarget(nameof(ExpenseDto.CompanyCategory))]
    [MapperIgnoreTarget(nameof(ExpenseDto.TruckId))]
    [MapperIgnoreTarget(nameof(ExpenseDto.Truck))]
    [MapperIgnoreTarget(nameof(ExpenseDto.TruckCategory))]
    [MapperIgnoreTarget(nameof(ExpenseDto.OdometerReading))]
    [MapperIgnoreTarget(nameof(ExpenseDto.Quantity))]
    [MapperIgnoreTarget(nameof(ExpenseDto.QuantityUnit))]
    [MapperIgnoreTarget(nameof(ExpenseDto.PurchaseJurisdiction))]
    [MapperIgnoreTarget(nameof(ExpenseDto.FuelCardProvider))]
    [MapperIgnoreTarget(nameof(ExpenseDto.PricePerUnit))]
    [MapperIgnoreTarget(nameof(ExpenseDto.VendorAddress))]
    [MapperIgnoreTarget(nameof(ExpenseDto.VendorPhone))]
    [MapperIgnoreTarget(nameof(ExpenseDto.RepairDescription))]
    [MapperIgnoreTarget(nameof(ExpenseDto.EstimatedCompletionDate))]
    [MapperIgnoreTarget(nameof(ExpenseDto.ActualCompletionDate))]
    [MapProperty(nameof(Expense.ReceiptBlobPath), nameof(ExpenseDto.ReceiptBlobPath), Use = nameof(@MappingConversions.DefaultToEmpty))]
    private static partial ExpenseDto MapExpense(Expense entity);

    extension(Expense entity)
    {
        public ExpenseDto ToDto(string? receiptDownloadUrl = null)
        {
            var dto = MapExpense(entity) with { ReceiptDownloadUrl = receiptDownloadUrl };
            return entity switch
            {
                CompanyExpense companyExpense => dto with
                {
                    Type = ExpenseType.Company,
                    CompanyCategory = companyExpense.Category
                },
                TruckExpense truckExpense => dto with
                {
                    Type = ExpenseType.Truck,
                    TruckId = truckExpense.TruckId,
                    Truck = truckExpense.Truck.ToDto(),
                    TruckCategory = truckExpense.Category,
                    OdometerReading = truckExpense.OdometerReading,
                    Quantity = truckExpense.Quantity,
                    QuantityUnit = truckExpense.QuantityUnit,
                    PurchaseJurisdiction = truckExpense.PurchaseJurisdiction,
                    FuelCardProvider = truckExpense.FuelCardProvider,
                    PricePerUnit = truckExpense.PricePerUnit
                },
                BodyShopExpense bodyShopExpense => dto with
                {
                    Type = ExpenseType.BodyShop,
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
}

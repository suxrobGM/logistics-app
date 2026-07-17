using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class FuelCardMapper
{
    public static FuelCardTransactionDto ToDto(this FuelCardTransaction entity)
    {
        return new FuelCardTransactionDto
        {
            Id = entity.Id,
            ProviderType = entity.ProviderType,
            ExternalTransactionId = entity.ExternalTransactionId,
            TransactionDate = entity.TransactionDate,
            Amount = entity.Amount,
            Quantity = entity.Quantity,
            QuantityUnit = entity.QuantityUnit,
            PricePerUnit = entity.PricePerUnit,
            ProductCategory = entity.ProductCategory,
            MerchantName = entity.MerchantName,
            MerchantCity = entity.MerchantCity,
            PurchaseJurisdiction = entity.PurchaseJurisdiction,
            CardNumberMasked = entity.CardNumberMasked,
            UnitNumber = entity.UnitNumber,
            DriverName = entity.DriverName,
            Status = entity.Status,
            TruckId = entity.TruckId,
            TruckNumber = entity.Truck?.Number,
            ExpenseId = entity.ExpenseId
        };
    }

    public static FuelCardDto ToDto(this FuelCard entity)
    {
        return new FuelCardDto
        {
            Id = entity.Id,
            ProviderType = entity.ProviderType,
            ExternalCardId = entity.ExternalCardId,
            UnitNumber = entity.UnitNumber,
            TruckId = entity.TruckId,
            TruckNumber = entity.Truck?.Number,
            IsActive = entity.IsActive
        };
    }
}

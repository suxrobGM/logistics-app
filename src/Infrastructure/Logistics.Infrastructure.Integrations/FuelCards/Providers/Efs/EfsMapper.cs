using System.Text.Json;
using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.FuelCards.Common;

namespace Logistics.Infrastructure.Integrations.FuelCards.Providers.Efs;

internal static class EfsMapper
{
    public static FuelCardTransactionData? ToTransactionData(EfsTransaction transaction)
    {
        if (string.IsNullOrEmpty(transaction.Id) || transaction.PostedDate is null || transaction.TotalAmount is null)
        {
            return null;
        }

        return new FuelCardTransactionData
        {
            ExternalTransactionId = transaction.Id,
            TransactionDate = DateTime.SpecifyKind(transaction.PostedDate.Value, DateTimeKind.Utc),
            Amount = transaction.TotalAmount.Value,
            Currency = transaction.Currency ?? "USD",
            Quantity = transaction.Gallons,
            QuantityUnit = transaction.Gallons is not null ? VolumeUnit.Gallons : null,
            PricePerUnit = transaction.UnitPrice,
            ProductCategory = transaction.ProductCode,
            MerchantName = transaction.LocationName,
            MerchantCity = transaction.LocationCity,
            PurchaseJurisdiction = JurisdictionMapper.FromMerchant(transaction.LocationCountry, transaction.LocationState),
            CardNumberMasked = transaction.CardNumber,
            ExternalCardId = transaction.CardNumber,
            UnitNumber = transaction.UnitNumber,
            DriverName = transaction.DriverName,
            RawJson = JsonSerializer.Serialize(transaction)
        };
    }
}

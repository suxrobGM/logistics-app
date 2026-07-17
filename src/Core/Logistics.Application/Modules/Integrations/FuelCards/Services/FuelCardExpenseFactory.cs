using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Integrations.FuelCards.Services;

internal static class FuelCardExpenseFactory
{
    /// <summary>
    /// Materializes a TruckExpense from a matched fuel card transaction. Created as Paid —
    /// card transactions are already-settled funds, so routing them through the Pending
    /// approval flow would only create busywork.
    /// </summary>
    public static TruckExpense CreateExpense(FuelCardTransaction transaction, Guid truckId)
    {
        return new TruckExpense
        {
            TruckId = truckId,
            Category = TruckExpenseCategory.Fuel,
            Status = ExpenseStatus.Paid,
            Amount = new Money { Amount = transaction.Amount.Amount, Currency = transaction.Amount.Currency },
            ExpenseDate = transaction.TransactionDate,
            VendorName = transaction.MerchantName,
            Quantity = transaction.Quantity,
            QuantityUnit = transaction.QuantityUnit,
            PricePerUnit = transaction.PricePerUnit,
            PurchaseJurisdiction = transaction.PurchaseJurisdiction,
            FuelCardProvider = transaction.ProviderType,
            ExternalTransactionId = transaction.ExternalTransactionId,
            Notes = $"Imported from {transaction.ProviderType} fuel card ({transaction.CardNumberMasked ?? transaction.ExternalTransactionId})"
        };
    }
}

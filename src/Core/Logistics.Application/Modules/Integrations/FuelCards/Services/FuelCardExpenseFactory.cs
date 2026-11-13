using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.FuelCards.Services;

internal static class FuelCardExpenseFactory
{
    /// <summary>True when the transaction is in the tenant currency. Expenses are stored in tenant currency, so transactions in other currencies get no expense.</summary>
    public static bool CanCreateExpense(FuelCardTransaction transaction, string tenantCurrency) =>
        string.Equals(transaction.Amount.Currency, tenantCurrency, StringComparison.OrdinalIgnoreCase);

    /// <summary>Creates a Paid fuel expense for the truck, since card transactions are already settled.</summary>
    public static TruckExpense CreateExpense(FuelCardTransaction transaction, Guid truckId)
    {
        return new TruckExpense
        {
            TruckId = truckId,
            Category = TruckExpenseCategory.Fuel,
            Status = ExpenseStatus.Paid,
            Amount = transaction.Amount.Amount,
            ExpenseDate = DateOnly.FromDateTime(transaction.TransactionDate),
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

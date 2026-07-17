using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record FuelCardTransactionDto
{
    public Guid Id { get; set; }
    public FuelCardProviderType ProviderType { get; set; }
    public required string ExternalTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public required Money Amount { get; set; }
    public decimal? Quantity { get; set; }
    public VolumeUnit? QuantityUnit { get; set; }
    public decimal? PricePerUnit { get; set; }
    public string? ProductCategory { get; set; }
    public string? MerchantName { get; set; }
    public string? MerchantCity { get; set; }
    public TaxJurisdiction? PurchaseJurisdiction { get; set; }
    public string? CardNumberMasked { get; set; }
    public string? UnitNumber { get; set; }
    public string? DriverName { get; set; }
    public FuelCardTransactionStatus Status { get; set; }
    public Guid? TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public Guid? ExpenseId { get; set; }
}

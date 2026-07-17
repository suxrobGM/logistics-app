namespace Logistics.Infrastructure.Integrations.FuelCards.Providers.Wex;

internal record WexTransactionsResponse
{
    public List<WexTransaction>? Transactions { get; set; }
    public int TotalCount { get; set; }
}

internal record WexTransaction
{
    public string? TransactionId { get; set; }
    public DateTime? TransactionDate { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public decimal? Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal? PricePerUnit { get; set; }
    public string? ProductDescription { get; set; }
    public string? MerchantName { get; set; }
    public string? MerchantCity { get; set; }
    public string? MerchantState { get; set; }
    public string? MerchantCountry { get; set; }
    public string? CardNumber { get; set; }
    public string? CardId { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
}

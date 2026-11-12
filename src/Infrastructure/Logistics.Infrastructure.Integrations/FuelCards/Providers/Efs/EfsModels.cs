namespace Logistics.Infrastructure.Integrations.FuelCards.Providers.Efs;

internal record EfsTransactionsResponse
{
    public List<EfsTransaction>? Data { get; set; }
    public int Total { get; set; }
}

internal record EfsTransaction
{
    public string? Id { get; set; }
    public DateTime? PostedDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Currency { get; set; }
    public decimal? Gallons { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? ProductCode { get; set; }
    public string? LocationName { get; set; }
    public string? LocationCity { get; set; }
    public string? LocationState { get; set; }
    public string? LocationCountry { get; set; }
    public string? CardNumber { get; set; }
    public string? UnitNumber { get; set; }
    public string? DriverName { get; set; }
}

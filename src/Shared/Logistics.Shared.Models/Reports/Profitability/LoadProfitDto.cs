namespace Logistics.Shared.Models;

/// <summary>One delivered load's revenue, costs and margin. Per-distance figures are in the tenant's distance unit.</summary>
public record LoadProfitDto
{
    public Guid LoadId { get; set; }
    public long LoadNumber { get; set; }
    public Guid? TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Lane { get; set; } = string.Empty;
    public DateTime DeliveredAt { get; set; }

    /// <summary>Loaded distance in metres.</summary>
    public double Distance { get; set; }

    public decimal Revenue { get; set; }
    public decimal DriverPay { get; set; }

    /// <summary>True when salaried or hourly pay was spread onto this load.</summary>
    public bool DriverPayEstimated { get; set; }

    /// <summary>The load's share of its truck's expenses, by category. Always an estimate.</summary>
    public IReadOnlyList<CostLineDto> TruckCosts { get; set; } = [];

    public decimal TruckCostsTotal { get; set; }
    public decimal FactoringFee { get; set; }
    public bool FactoringFeeEstimated { get; set; }
    public decimal Margin { get; set; }
    public decimal? MarginPerDistance { get; set; }
}

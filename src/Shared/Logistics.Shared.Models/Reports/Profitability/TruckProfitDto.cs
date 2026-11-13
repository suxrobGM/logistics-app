namespace Logistics.Shared.Models;

/// <summary>A truck's delivered loads grouped. Per-distance figures are in the tenant's distance unit.</summary>
public record TruckProfitDto
{
    public Guid TruckId { get; set; }
    public string TruckNumber { get; set; } = string.Empty;
    public int LoadCount { get; set; }

    /// <summary>Metres.</summary>
    public double LoadedDistance { get; set; }

    /// <summary>Metres driven per the IFTA mileage log; null when the truck has none.</summary>
    public double? ActualDistance { get; set; }

    public double? DeadheadPercent { get; set; }
    public decimal Revenue { get; set; }
    public decimal DriverPay { get; set; }
    public IReadOnlyList<CostLineDto> TruckCosts { get; set; } = [];
    public decimal TruckCostsTotal { get; set; }
    public decimal FactoringFee { get; set; }
    public decimal Margin { get; set; }

    /// <summary>Driver pay plus truck costs over actual distance, falling back to loaded distance.</summary>
    public decimal? CostPerDistance { get; set; }
}

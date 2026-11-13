namespace Logistics.Shared.Models;

/// <summary>
/// Company totals for a period. NetProfit = LoadMargin - UnallocatedTruckCosts - UnallocatedDriverPay - Overhead.
/// </summary>
public record ProfitabilitySummaryDto
{
    public int LoadCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal DriverPay { get; set; }
    public decimal TruckCosts { get; set; }
    public decimal FactoringFees { get; set; }
    public decimal LoadMargin { get; set; }

    /// <summary>Expenses of trucks that delivered nothing in the period.</summary>
    public decimal UnallocatedTruckCosts { get; set; }

    /// <summary>Salaried or hourly pay of drivers who delivered nothing in the period.</summary>
    public decimal UnallocatedDriverPay { get; set; }

    /// <summary>Approved or paid company expenses.</summary>
    public decimal Overhead { get; set; }

    public decimal NetProfit { get; set; }

    /// <summary>Fuel card transactions not matched to a truck yet; shown for information, not in the totals.</summary>
    public decimal UnassignedFuel { get; set; }
}

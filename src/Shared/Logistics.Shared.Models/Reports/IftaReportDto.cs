namespace Logistics.Shared.Models;

public record IftaReportDto
{
    public int Year { get; set; }

    /// <summary>Calendar quarter, 1-4.</summary>
    public int Quarter { get; set; }

    /// <summary>True when served from an immutable quarter-close snapshot.</summary>
    public bool IsClosed { get; set; }

    public DateTime? ClosedAt { get; set; }

    public decimal TotalMiles { get; set; }

    /// <summary>Total fuel purchased in the quarter (gallons), including purchases without a jurisdiction.</summary>
    public decimal TotalGallons { get; set; }

    /// <summary>Fleet average MPG = total miles / total gallons.</summary>
    public decimal AverageMpg { get; set; }

    /// <summary>Sum of computable per-jurisdiction tax due (negative = net credit).</summary>
    public decimal TotalTaxDue { get; set; }

    /// <summary>True when any jurisdiction row has no published rate for the quarter.</summary>
    public bool HasMissingRates { get; set; }

    public List<IftaJurisdictionRowDto> Jurisdictions { get; set; } = [];
}

public record IftaJurisdictionRowDto
{
    /// <summary>ISO 3166-1 alpha-2 country code.</summary>
    public required string CountryCode { get; set; }

    /// <summary>State/province code (e.g. "TX", "ON").</summary>
    public required string Region { get; set; }

    /// <summary>Miles driven in the jurisdiction.</summary>
    public decimal Miles { get; set; }

    /// <summary>Gallons purchased in the jurisdiction.</summary>
    public decimal PurchasedGallons { get; set; }

    /// <summary>Miles / fleet MPG — the gallons deemed consumed in the jurisdiction.</summary>
    public decimal TaxableGallons { get; set; }

    /// <summary>TaxableGallons - PurchasedGallons; negative = credit.</summary>
    public decimal NetTaxableGallons { get; set; }

    public decimal? RatePerGallon { get; set; }

    public decimal? SurchargeRatePerGallon { get; set; }

    /// <summary>
    /// NetTaxableGallons × rate + TaxableGallons × surcharge. Null when the rate is missing.
    /// </summary>
    public decimal? TaxDue { get; set; }

    /// <summary>No published rate exists for this jurisdiction/quarter — filing needs it added.</summary>
    public bool RateMissing { get; set; }
}

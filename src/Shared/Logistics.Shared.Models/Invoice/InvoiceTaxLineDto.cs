namespace Logistics.Shared.Models;

public record InvoiceTaxLineDto
{
    public decimal RatePercent { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public TaxJurisdictionDto Jurisdiction { get; set; } = null!;
    public string? TaxCode { get; set; }
    public string? Description { get; set; }
}

public record TaxJurisdictionDto
{
    public string CountryCode { get; set; } = null!;
    public string? Region { get; set; }
}

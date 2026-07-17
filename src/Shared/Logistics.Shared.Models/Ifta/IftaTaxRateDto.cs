namespace Logistics.Shared.Models;

public record IftaTaxRateDto
{
    public Guid Id { get; set; }
    public TaxJurisdictionDto Jurisdiction { get; set; } = null!;
    public int Year { get; set; }
    public int Quarter { get; set; }
    public decimal RatePerGallon { get; set; }
    public decimal? SurchargeRatePerGallon { get; set; }
    public DateTime CreatedAt { get; set; }
}

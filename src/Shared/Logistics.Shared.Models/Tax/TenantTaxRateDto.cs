namespace Logistics.Shared.Models;

public record TenantTaxRateDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public TaxJurisdictionDto Jurisdiction { get; set; } = null!;
    public decimal RatePercent { get; set; }
    public string? TaxCode { get; set; }
    public string? Description { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }
}

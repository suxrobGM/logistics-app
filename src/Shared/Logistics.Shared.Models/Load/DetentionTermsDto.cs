namespace Logistics.Shared.Models;

/// <summary>
/// Detention free time and hourly rate. On a customer, null fields fall back to the tenant's; on the
/// tenant, a null free time means the 2-hour default and a null rate means detention is not billed.
/// </summary>
public record DetentionTermsDto
{
    public int? FreeMinutes { get; set; }
    public decimal? RatePerHour { get; set; }
}

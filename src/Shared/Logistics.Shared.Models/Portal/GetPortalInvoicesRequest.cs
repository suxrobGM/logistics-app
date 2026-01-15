namespace Logistics.Shared.Models.Portal;

/// <summary>
/// Request parameters for getting portal invoices.
/// </summary>
public record GetPortalInvoicesRequest
{
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? OrderBy { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

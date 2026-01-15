namespace Logistics.Shared.Models.Portal;

/// <summary>
/// Request parameters for getting portal loads.
/// </summary>
public record GetPortalLoadsRequest
{
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? OrderBy { get; init; }
    public bool OnlyActiveLoads { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

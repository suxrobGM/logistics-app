using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

/// <summary>Delivered loads with their margin, worst first unless <c>OrderBy</c> says otherwise.</summary>
public class LoadsProfitabilityQuery : PagedIntervalQuery, IQuery<PagedResult<LoadProfitDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Lane { get; set; }
}

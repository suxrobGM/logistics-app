using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class TrucksProfitabilityQuery : IntervalQuery, IQuery<Result<List<TruckProfitDto>>>
{
    public Guid? TruckId { get; set; }
}

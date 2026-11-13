using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class LanesProfitabilityQuery : IntervalQuery, IQuery<Result<List<LaneProfitDto>>>;

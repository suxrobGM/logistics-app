using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class ProfitabilitySummaryQuery : IntervalQuery, IQuery<Result<ProfitabilitySummaryDto>>;

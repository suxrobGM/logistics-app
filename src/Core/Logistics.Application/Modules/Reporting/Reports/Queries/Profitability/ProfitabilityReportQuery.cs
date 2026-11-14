using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class ProfitabilityReportQuery : DateRangeQuery, IQuery<Result<ProfitabilityReportDto>>;

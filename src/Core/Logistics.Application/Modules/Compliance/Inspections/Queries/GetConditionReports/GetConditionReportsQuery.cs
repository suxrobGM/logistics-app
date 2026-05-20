using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Inspections.Queries;

public class GetConditionReportsQuery : IQuery<Result<List<ConditionReportDto>>>
{
    public Guid? LoadId { get; set; }
    public Guid? ConditionReportId { get; set; }
}

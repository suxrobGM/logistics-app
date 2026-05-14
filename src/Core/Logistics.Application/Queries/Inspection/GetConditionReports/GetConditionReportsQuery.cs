using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetConditionReportsQuery : IQuery<Result<List<ConditionReportDto>>>
{
    public Guid? LoadId { get; set; }
    public Guid? VehicleConditionReportId { get; set; }
}

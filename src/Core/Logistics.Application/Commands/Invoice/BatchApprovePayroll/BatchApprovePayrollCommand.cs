using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class BatchApprovePayrollCommand : IAppRequest
{
    public required List<Guid> Ids { get; set; }
    public string? Notes { get; set; }
}

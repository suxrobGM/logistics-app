using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class SubmitPayrollForApprovalCommand : IAppRequest
{
    public Guid Id { get; set; }
}

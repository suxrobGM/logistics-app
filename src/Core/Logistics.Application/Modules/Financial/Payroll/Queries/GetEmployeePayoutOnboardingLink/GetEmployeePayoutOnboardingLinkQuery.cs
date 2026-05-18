using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Queries;

public class GetEmployeePayoutOnboardingLinkQuery : IQuery<Result<EmployeePayoutOnboardingLinkDto>>
{
    public Guid EmployeeId { get; set; }
    public required string ReturnUrl { get; set; }
    public required string RefreshUrl { get; set; }
}

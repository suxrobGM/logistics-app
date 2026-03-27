using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetEmployeePayoutOnboardingLinkQuery : IAppRequest<Result<EmployeePayoutOnboardingLinkDto>>
{
    public Guid EmployeeId { get; set; }
    public required string ReturnUrl { get; set; }
    public required string RefreshUrl { get; set; }
}

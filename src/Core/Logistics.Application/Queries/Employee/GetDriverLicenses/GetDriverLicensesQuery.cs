using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetDriverLicensesQuery : IAppRequest<Result<IList<DriverLicenseDto>>>
{
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// When false (default), revoked licenses are excluded from the result.
    /// </summary>
    public bool IncludeRevoked { get; set; }
}

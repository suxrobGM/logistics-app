using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetExpiringLicensesQuery : IAppRequest<Result<IList<DriverLicenseDto>>>
{
    /// <summary>
    /// Window in days. Returns active licenses expiring within this many days from now.
    /// </summary>
    public int WindowDays { get; set; } = 30;
}

using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public record GetExpiringCertificationsQuery : IAppRequest<Result<List<DriverCertificationDto>>>
{
    public int DaysUntilExpiration { get; set; } = 30;
}

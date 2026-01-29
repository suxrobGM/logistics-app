using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetExpiringCertificationsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetExpiringCertificationsQuery, Result<List<DriverCertificationDto>>>
{
    public Task<Result<List<DriverCertificationDto>>> Handle(GetExpiringCertificationsQuery req, CancellationToken ct)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(req.DaysUntilExpiration);

        var certifications = tenantUow.Repository<DriverCertification>()
            .Query()
            .Where(c => c.ExpirationDate <= cutoffDate && c.ExpirationDate >= DateTime.UtcNow)
            .OrderBy(c => c.ExpirationDate)
            .Select(c => c.ToDto())
            .ToList();

        return Task.FromResult(Result<List<DriverCertificationDto>>.Ok(certifications));
    }
}

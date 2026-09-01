using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.ProductLicense.Commands;

internal sealed class RecordProductLicenseHeartbeatHandler(IMasterUnitOfWork masterUow)
    : IAppRequestHandler<RecordProductLicenseHeartbeatCommand, Result>
{
    public async Task<Result> Handle(RecordProductLicenseHeartbeatCommand req, CancellationToken ct)
    {
        var report = req.Report;
        var repo = masterUow.Repository<ProductLicenseHeartbeat>();
        var now = DateTime.UtcNow;
        var row = await repo.GetAsync(x => x.InstanceId == report.InstanceId, ct);

        if (row is null)
        {
            row = new ProductLicenseHeartbeat
            {
                InstanceId = report.InstanceId,
                Hostname = report.Hostname,
                Version = report.Version,
                KeyId = report.KeyId,
                Licensee = report.Licensee,
                TenantCount = report.TenantCount,
                FirstSeenAt = now,
                LastSeenAt = now
            };
            await repo.AddAsync(row, ct);
        }
        else
        {
            row.Touch(report.Hostname, report.Version, report.KeyId, report.Licensee, report.TenantCount, now);
            repo.Update(row);
        }

        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

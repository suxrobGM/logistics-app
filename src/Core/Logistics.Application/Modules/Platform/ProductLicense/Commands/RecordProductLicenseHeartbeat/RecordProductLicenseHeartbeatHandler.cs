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
        var repo = masterUow.Repository<LicenseHeartbeat>();
        var now = DateTime.UtcNow;
        var row = await repo.GetAsync(x => x.InstanceId == req.InstanceId, ct);

        if (row is null)
        {
            row = new LicenseHeartbeat
            {
                InstanceId = req.InstanceId,
                Hostname = req.Hostname,
                Version = req.Version,
                KeyId = req.KeyId,
                Licensee = req.Licensee,
                TenantCount = req.TenantCount,
                FirstSeenAt = now,
                LastSeenAt = now
            };
            await repo.AddAsync(row, ct);
        }
        else
        {
            row.Touch(req.Hostname, req.Version, req.KeyId, req.Licensee, req.TenantCount, now);
            repo.Update(row);
        }

        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

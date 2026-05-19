using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Application.Modules.Compliance.Privacy.Commands;

internal sealed class RequestDataExportHandler(
    IMasterUnitOfWork masterUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<RequestDataExportCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RequestDataExportCommand req, CancellationToken ct)
    {
        var userId = currentUserService.GetUserId();
        if (userId is null)
        {
            return Result<Guid>.Fail("User not authenticated.");
        }

        var since = DateTime.UtcNow - TimeSpan.FromDays(1);
        var recentCount = await masterUow.Repository<DataExportRequest>()
            .Query()
            .Where(r => r.UserId == userId.Value && r.RequestedAt >= since)
            .CountAsync(ct);

        if (recentCount >= PrivacyDefaults.ExportRateLimitPerDay)
        {
            return Result<Guid>.Fail(
                $"You can request at most {PrivacyDefaults.ExportRateLimitPerDay} data export per 24 hours.");
        }

        var request = new DataExportRequest
        {
            UserId = userId.Value,
            Status = DataExportStatus.Pending
        };

        await masterUow.Repository<DataExportRequest>().AddAsync(request, ct);
        await masterUow.SaveChangesAsync(ct);

        return Result<Guid>.Ok(request.Id);
    }
}

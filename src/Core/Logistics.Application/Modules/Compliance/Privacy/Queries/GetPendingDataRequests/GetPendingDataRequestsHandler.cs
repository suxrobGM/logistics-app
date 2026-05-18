using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Privacy.Queries;

internal sealed class GetPendingDataRequestsHandler(IMasterUnitOfWork masterUow)
    : IAppRequestHandler<GetPendingDataRequestsQuery, Result<PendingDataRequestsDto>>
{
    public async Task<Result<PendingDataRequestsDto>> Handle(GetPendingDataRequestsQuery req, CancellationToken ct)
    {
        var pendingExports = await masterUow.Repository<DataExportRequest>()
            .Query()
            .Where(r => r.Status == DataExportStatus.Pending || r.Status == DataExportStatus.Processing)
            .OrderBy(r => r.RequestedAt)
            .ToListAsync(ct);

        var pendingDeletions = await masterUow.Repository<DataDeletionRequest>()
            .Query()
            .Where(r => r.Status == DataDeletionStatus.Pending)
            .OrderBy(r => r.ScheduledFor)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var dto = new PendingDataRequestsDto
        {
            PendingExports = pendingExports.Select(r => r.ToDto()).ToList(),
            PendingDeletions = pendingDeletions.Select(r => r.ToDto(now)).ToList()
        };

        return Result<PendingDataRequestsDto>.Ok(dto);
    }
}

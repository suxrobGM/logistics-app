using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Application.Modules.Compliance.Privacy.Queries;

internal sealed class GetConsentHistoryHandler(
    IMasterUnitOfWork masterUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<GetConsentHistoryQuery, Result<List<ConsentRecordDto>>>
{
    public async Task<Result<List<ConsentRecordDto>>> Handle(GetConsentHistoryQuery req, CancellationToken ct)
    {
        var userId = currentUserService.GetUserId();
        if (userId is null)
        {
            return Result<List<ConsentRecordDto>>.Fail("User not authenticated.");
        }

        var records = await masterUow.Repository<ConsentRecord>()
            .Query()
            .Where(r => r.UserId == userId.Value)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync(ct);

        return Result<List<ConsentRecordDto>>.Ok(records.Select(r => r.ToDto()).ToList());
    }
}

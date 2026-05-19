using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Application.Modules.Compliance.Privacy.Queries;

internal sealed class GetMyDataDeletionsHandler(
    IMasterUnitOfWork masterUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<GetMyDataDeletionsQuery, Result<List<DataDeletionRequestDto>>>
{
    public async Task<Result<List<DataDeletionRequestDto>>> Handle(GetMyDataDeletionsQuery req, CancellationToken ct)
    {
        var userId = currentUserService.GetUserId();
        if (userId is null)
        {
            return Result<List<DataDeletionRequestDto>>.Fail("User not authenticated.");
        }

        var requests = await masterUow.Repository<DataDeletionRequest>()
            .Query()
            .Where(r => r.UserId == userId.Value)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var dtos = requests.Select(r => r.ToDto(now)).ToList();
        return Result<List<DataDeletionRequestDto>>.Ok(dtos);
    }
}

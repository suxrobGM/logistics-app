using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetApiKeysHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetApiKeysQuery, Result<List<ApiKeyDto>>>
{
    public async Task<Result<List<ApiKeyDto>>> Handle(GetApiKeysQuery req, CancellationToken ct)
    {
        var apiKeys = await tenantUow.Repository<ApiKey>()
            .GetListAsync(ct: ct);

        var dtos = apiKeys
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new ApiKeyDto(
                k.Id,
                k.Name,
                k.KeyPrefix,
                k.CreatedAt,
                k.LastUsedAt,
                k.IsActive))
            .ToList();

        return Result<List<ApiKeyDto>>.Ok(dtos);
    }
}

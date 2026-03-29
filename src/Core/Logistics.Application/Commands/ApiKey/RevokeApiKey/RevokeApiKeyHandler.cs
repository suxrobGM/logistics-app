using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class RevokeApiKeyHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<RevokeApiKeyCommand, Result>
{
    public async Task<Result> Handle(RevokeApiKeyCommand req, CancellationToken ct)
    {
        var apiKey = await tenantUow.Repository<ApiKey>()
            .GetByIdAsync(req.Id, ct);

        if (apiKey is null)
            return Result.Fail("API key not found.");

        tenantUow.Repository<ApiKey>().Delete(apiKey);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

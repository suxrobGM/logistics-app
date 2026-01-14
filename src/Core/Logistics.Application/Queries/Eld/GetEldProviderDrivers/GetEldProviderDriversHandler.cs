using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetEldProviderDriversHandler(
    ITenantUnitOfWork tenantUow,
    IEldProviderFactory eldProviderFactory)
    : IAppRequestHandler<GetEldProviderDriversQuery, Result<List<EldDriverDto>>>
{
    public async Task<Result<List<EldDriverDto>>> Handle(GetEldProviderDriversQuery req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<EldProviderConfiguration>()
            .GetByIdAsync(req.ProviderId, ct);

        if (config is null)
        {
            return Result<List<EldDriverDto>>.Fail("ELD provider configuration not found");
        }

        var providerService = eldProviderFactory.GetProvider(config);
        var drivers = await providerService.GetAllDriversAsync();

        return Result<List<EldDriverDto>>.Ok(drivers.ToList());
    }
}

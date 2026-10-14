using Logistics.Application.Modules.Compliance.Eld.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

internal sealed class GetEldMappingSuggestionsHandler(
    ITenantUnitOfWork tenantUow,
    IEldMappingSuggestionService suggestionService)
    : IRequestHandler<GetEldMappingSuggestionsQuery, Result<EldMappingSuggestionsDto>>
{
    public async Task<Result<EldMappingSuggestionsDto>> Handle(GetEldMappingSuggestionsQuery req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<EldProviderConfiguration>().GetByIdAsync(req.ProviderId, ct);
        return config is null
            ? Result<EldMappingSuggestionsDto>.Fail("ELD provider configuration not found")
            : Result<EldMappingSuggestionsDto>.Ok(await suggestionService.GetSuggestionsAsync(config, ct));
    }
}

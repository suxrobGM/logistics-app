using Logistics.Application.Abstractions;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Tax;

namespace Logistics.Application.Modules.Financial.Tax.Queries;

internal sealed class GetTaxJurisdictionsHandler(ITaxJurisdictionsProvider provider)
    : IAppRequestHandler<GetTaxJurisdictionsQuery, Result<IReadOnlyList<TaxJurisdictionInfoDto>>>
{
    public Task<Result<IReadOnlyList<TaxJurisdictionInfoDto>>> Handle(
        GetTaxJurisdictionsQuery req, CancellationToken ct)
    {
        var jurisdictions = provider.GetSupportedJurisdictions()
            .Select(j => new TaxJurisdictionInfoDto
            {
                CountryCode = j.CountryCode,
                Region = j.Region,
                DisplayName = j.DisplayName,
                DefaultRatePercent = j.DefaultRatePercent,
                Source = j.Source
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<TaxJurisdictionInfoDto>>.Ok(jurisdictions));
    }
}

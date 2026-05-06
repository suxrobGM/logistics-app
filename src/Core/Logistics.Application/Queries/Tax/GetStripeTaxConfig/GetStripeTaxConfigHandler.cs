using Logistics.Application.Abstractions;
using Logistics.Application.Services.Tax;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetStripeTaxConfigHandler(IStripeTaxConfigService configService)
    : IAppRequestHandler<GetStripeTaxConfigQuery, Result<StripeTaxConfigDto>>
{
    public async Task<Result<StripeTaxConfigDto>> Handle(GetStripeTaxConfigQuery req, CancellationToken ct)
    {
        var defaultCode = await configService.GetDefaultTaxCodeAsync(ct);
        var registrations = await configService.ListRegistrationsAsync(ct);
        var codes = req.IncludeTaxCodes
            ? await configService.ListTaxCodesAsync(ct)
            : [];

        return Result<StripeTaxConfigDto>.Ok(new StripeTaxConfigDto
        {
            DefaultTaxCode = defaultCode,
            Registrations = registrations
                .Select(r => new StripeTaxRegistrationDto(
                    r.Id, r.Country, r.State, r.Status, r.ActiveFrom, r.ExpiresAt))
                .ToList(),
            TaxCodes = codes
                .Select(c => new StripeTaxCodeDto(c.Id, c.Name, c.Description))
                .ToList()
        });
    }
}

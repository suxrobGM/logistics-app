using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Commands;

internal sealed class CreateIftaTaxRateHandler(
    IMasterUnitOfWork masterUow,
    ILogger<CreateIftaTaxRateHandler> logger) : IAppRequestHandler<CreateIftaTaxRateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateIftaTaxRateCommand req, CancellationToken ct)
    {
        var country = req.CountryCode.ToUpperInvariant();
        var region = string.IsNullOrWhiteSpace(req.Region) ? null : req.Region.ToUpperInvariant();

        // (Year, Quarter, Jurisdiction) uniqueness lives here: complex-type members
        // cannot participate in a DB unique index (see IftaTaxRateEntityConfiguration)
        var duplicate = await masterUow.Repository<IftaTaxRate>().GetAsync(
            x => x.Year == req.Year && x.Quarter == req.Quarter &&
                 x.Jurisdiction.CountryCode == country && x.Jurisdiction.Region == region, ct);

        if (duplicate is not null)
        {
            return Result<Guid>.Fail(
                $"An IFTA tax rate for {duplicate.Jurisdiction} {req.Year} Q{req.Quarter} already exists");
        }

        var rate = new IftaTaxRate
        {
            Jurisdiction = new TaxJurisdiction { CountryCode = country, Region = region },
            Year = req.Year,
            Quarter = req.Quarter,
            RatePerGallon = req.RatePerGallon,
            SurchargeRatePerGallon = req.SurchargeRatePerGallon,
        };

        await masterUow.Repository<IftaTaxRate>().AddAsync(rate, ct);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created IFTA tax rate {Jurisdiction} {Year} Q{Quarter}", rate.Jurisdiction, req.Year, req.Quarter);
        return Result<Guid>.Ok(rate.Id);
    }
}

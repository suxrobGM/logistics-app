using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Commands;

internal sealed class UpdateIftaTaxRateHandler(
    IMasterUnitOfWork masterUow,
    ILogger<UpdateIftaTaxRateHandler> logger) : IAppRequestHandler<UpdateIftaTaxRateCommand, Result>
{
    public async Task<Result> Handle(UpdateIftaTaxRateCommand req, CancellationToken ct)
    {
        var rate = await masterUow.Repository<IftaTaxRate>().GetByIdAsync(req.Id, ct);

        if (rate is null)
        {
            return Result.Fail($"IFTA tax rate with ID '{req.Id}' not found");
        }

        var country = req.CountryCode.ToUpperInvariant();
        var region = string.IsNullOrWhiteSpace(req.Region) ? null : req.Region.ToUpperInvariant();

        // (Year, Quarter, Jurisdiction) uniqueness lives here: complex-type members
        // cannot participate in a DB unique index (see IftaTaxRateEntityConfiguration)
        var duplicate = await masterUow.Repository<IftaTaxRate>().GetAsync(
            x => x.Id != req.Id && x.Year == req.Year && x.Quarter == req.Quarter &&
                 x.Jurisdiction.CountryCode == country && x.Jurisdiction.Region == region, ct);

        if (duplicate is not null)
        {
            return Result.Fail(
                $"An IFTA tax rate for {duplicate.Jurisdiction} {req.Year} Q{req.Quarter} already exists");
        }

        rate.Jurisdiction = new TaxJurisdiction { CountryCode = country, Region = region };
        rate.Year = req.Year;
        rate.Quarter = req.Quarter;
        rate.RatePerGallon = req.RatePerGallon;
        rate.SurchargeRatePerGallon = req.SurchargeRatePerGallon;
        rate.UpdatedAt = DateTime.UtcNow;

        masterUow.Repository<IftaTaxRate>().Update(rate);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Updated IFTA tax rate {Id} for {Jurisdiction} {Year} Q{Quarter}",
            req.Id, rate.Jurisdiction, req.Year, req.Quarter);
        return Result.Ok();
    }
}

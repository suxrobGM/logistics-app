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

        var jurisdiction = TaxJurisdiction.Create(req.CountryCode, req.Region);

        var conflict = await IftaTaxRateUniqueness.FindConflictAsync(
            masterUow, jurisdiction, req.Year, req.Quarter, excludeId: req.Id, ct);

        if (conflict is not null)
        {
            return Result.Fail(conflict);
        }

        rate.Jurisdiction = jurisdiction;
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

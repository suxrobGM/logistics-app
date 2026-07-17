using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Commands;

internal sealed class DeleteIftaTaxRateHandler(
    IMasterUnitOfWork masterUow,
    ILogger<DeleteIftaTaxRateHandler> logger) : IAppRequestHandler<DeleteIftaTaxRateCommand, Result>
{
    public async Task<Result> Handle(DeleteIftaTaxRateCommand req, CancellationToken ct)
    {
        var rate = await masterUow.Repository<IftaTaxRate>().GetByIdAsync(req.Id, ct);

        if (rate is null)
        {
            return Result.Fail($"IFTA tax rate with ID '{req.Id}' not found");
        }

        masterUow.Repository<IftaTaxRate>().Delete(rate);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Deleted IFTA tax rate {Id}", req.Id);
        return Result.Ok();
    }
}

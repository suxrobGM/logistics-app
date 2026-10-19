using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Trailers.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trailers.Commands;

internal sealed class UpdateTrailerHandler(ITenantUnitOfWork tenantUow, ITrailerHookService trailerHookService)
    : IRequestHandler<UpdateTrailerCommand, Result>
{
    public async Task<Result> Handle(UpdateTrailerCommand req, CancellationToken ct)
    {
        var repository = tenantUow.Repository<Trailer>();
        var trailer = await repository.GetByIdAsync(req.Id, ct);
        if (trailer is null)
        {
            return Result.Fail($"Could not find a trailer with ID '{req.Id}'");
        }

        var number = req.Number.Trim();
        var conflict = await repository.GetAsync(i => i.Number == number && i.Id != trailer.Id, ct);
        if (conflict is not null)
        {
            return Result.Fail($"A trailer with number '{number}' already exists");
        }

        trailer.Number = number;
        trailer.Type = req.Type;
        trailer.Status = req.Status;
        trailer.Vin = req.Vin;
        trailer.LicensePlate = req.LicensePlate;
        trailer.LicensePlateState = req.LicensePlateState;
        trailer.Year = req.Year;
        trailer.Make = req.Make;
        trailer.Model = req.Model;
        trailer.LengthMeters = req.LengthMeters;
        trailer.Notes = req.Notes;

        if (trailer.Vin is not null && await repository.GetAsync(i => i.Vin == trailer.Vin && i.Id != trailer.Id, ct) is not null)
        {
            return Result.Fail($"A trailer with VIN '{trailer.Vin}' already exists");
        }

        await trailerHookService.UnhookUnlessActiveAsync(trailer, ct);

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

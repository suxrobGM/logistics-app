using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trailers.Commands;

internal sealed class CreateTrailerHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<CreateTrailerCommand, Result<TrailerDto>>
{
    public async Task<Result<TrailerDto>> Handle(CreateTrailerCommand req, CancellationToken ct)
    {
        var number = req.Number.Trim();
        var existing = await tenantUow.Repository<Trailer>().GetAsync(i => i.Number == number, ct);
        if (existing is not null)
        {
            return Result<TrailerDto>.Fail($"A trailer with number '{number}' already exists");
        }

        var trailer = new Trailer
        {
            Number = number,
            Type = req.Type,
            Status = req.Status,
            Vin = req.Vin,
            LicensePlate = req.LicensePlate,
            LicensePlateState = req.LicensePlateState,
            Year = req.Year,
            Make = req.Make,
            Model = req.Model,
            LengthMeters = req.LengthMeters,
            Notes = req.Notes
        };

        await tenantUow.Repository<Trailer>().AddAsync(trailer, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result<TrailerDto>.Ok(trailer.ToDto());
    }
}

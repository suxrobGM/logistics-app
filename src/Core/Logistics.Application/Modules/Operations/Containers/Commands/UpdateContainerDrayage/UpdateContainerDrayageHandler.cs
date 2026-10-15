using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

internal sealed class UpdateContainerDrayageHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<UpdateContainerDrayageCommand, Result>
{
    public async Task<Result> Handle(UpdateContainerDrayageCommand req, CancellationToken ct)
    {
        var container = await tenantUow.Repository<Container>().GetByIdAsync(req.Id, ct);
        if (container is null)
        {
            return Result.Fail($"Could not find container with ID '{req.Id}'");
        }

        Terminal? emptyReturnTerminal = null;
        if (req.EmptyReturnTerminalId is { } terminalId)
        {
            emptyReturnTerminal = await tenantUow.Repository<Terminal>().GetByIdAsync(terminalId, ct);
            if (emptyReturnTerminal is null)
            {
                return Result.Fail($"Could not find terminal with ID '{terminalId}'");
            }
        }

        container.SteamshipLineScac = Trimmed(req.SteamshipLineScac)?.ToUpperInvariant();
        container.SteamshipLineName = Trimmed(req.SteamshipLineName);
        container.LastFreeDay = req.LastFreeDay;
        container.DemurrageRatePerDay = req.DemurrageRatePerDay;
        container.EmptyReturnDeadline = req.EmptyReturnDeadline;
        container.PerDiemRatePerDay = req.PerDiemRatePerDay;
        container.PickedUpAt = req.PickedUpAt;
        container.EmptyReturnTerminalId = emptyReturnTerminal?.Id;
        container.EmptyReturnTerminal = emptyReturnTerminal;
        container.CustomsHold = req.CustomsHold;
        container.FreightHold = req.FreightHold;
        container.LineHold = req.LineHold;
        container.OtherHold = req.OtherHold;
        container.AvailableForPickupAt = req.AvailableForPickupAt;
        container.PickupAppointmentNumber = Trimmed(req.PickupAppointmentNumber);
        container.PickupAppointmentFrom = req.PickupAppointmentFrom;
        container.PickupAppointmentTo = req.PickupAppointmentTo;
        container.ReturnAppointmentNumber = Trimmed(req.ReturnAppointmentNumber);
        container.ReturnAppointmentFrom = req.ReturnAppointmentFrom;
        container.ReturnAppointmentTo = req.ReturnAppointmentTo;
        container.ChassisNumber = Trimmed(req.ChassisNumber)?.ToUpperInvariant();
        container.ChassisProvider = req.ChassisProvider;
        container.ChassisRatePerDay = req.ChassisRatePerDay;

        tenantUow.Repository<Container>().Update(container);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private static string? Trimmed(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

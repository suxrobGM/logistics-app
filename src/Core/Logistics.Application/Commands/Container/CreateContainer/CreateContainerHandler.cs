using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateContainerHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateContainerCommand, Result<ContainerDto>>
{
    public async Task<Result<ContainerDto>> Handle(CreateContainerCommand req, CancellationToken ct)
    {
        var existing = await tenantUow.Repository<Container>().GetAsync(i => i.Number == req.Number, ct);

        if (existing is not null)
        {
            return Result<ContainerDto>.Fail($"A container with number '{req.Number}' already exists");
        }

        Terminal? terminal = null;
        if (req.CurrentTerminalId.HasValue)
        {
            terminal = await tenantUow.Repository<Terminal>().GetByIdAsync(req.CurrentTerminalId.Value, ct);
            if (terminal is null)
            {
                return Result<ContainerDto>.Fail($"Could not find terminal with ID '{req.CurrentTerminalId}'");
            }
        }

        var container = new Container
        {
            Number = req.Number,
            IsoType = req.IsoType,
            SealNumber = req.SealNumber,
            BookingReference = req.BookingReference,
            BillOfLadingNumber = req.BillOfLadingNumber,
            IsLaden = req.IsLaden,
            GrossWeight = req.GrossWeight,
            CurrentTerminalId = terminal?.Id,
            CurrentTerminal = terminal,
            Notes = req.Notes
        };

        // Initial status - bypass state machine via force, since the container starts at the requested status.
        if (req.Status != ContainerStatus.Empty)
        {
            container.UpdateStatus(req.Status, force: true);
        }

        await tenantUow.Repository<Container>().AddAsync(container, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<ContainerDto>.Ok(container.ToDto());
    }
}

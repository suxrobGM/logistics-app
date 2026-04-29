using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateContainerStatusHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateContainerStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateContainerStatusCommand req, CancellationToken ct)
    {
        var container = await tenantUow.Repository<Container>().GetByIdAsync(req.Id, ct);
        if (container is null)
        {
            return Result.Fail($"Could not find container with ID '{req.Id}'");
        }

        Terminal? terminal = null;
        if (req.TargetStatus is ContainerStatus.AtPort or ContainerStatus.Returned)
        {
            if (!req.TerminalId.HasValue)
            {
                return Result.Fail($"TerminalId is required when transitioning to '{req.TargetStatus}'.");
            }

            terminal = await tenantUow.Repository<Terminal>().GetByIdAsync(req.TerminalId.Value, ct);
            if (terminal is null)
            {
                return Result.Fail($"Could not find terminal with ID '{req.TerminalId}'");
            }
        }

        try
        {
            switch (req.TargetStatus)
            {
                case ContainerStatus.Loaded:
                    container.MarkAsLoaded();
                    break;
                case ContainerStatus.Empty:
                    container.MarkAsEmpty();
                    break;
                case ContainerStatus.AtPort:
                    container.MarkAtPort(terminal!);
                    break;
                case ContainerStatus.InTransit:
                    container.MarkInTransit();
                    break;
                case ContainerStatus.Delivered:
                    container.MarkDelivered();
                    break;
                case ContainerStatus.Returned:
                    container.MarkReturned(terminal!);
                    break;
                default:
                    return Result.Fail($"Unsupported target status '{req.TargetStatus}'.");
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        tenantUow.Repository<Container>().Update(container);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

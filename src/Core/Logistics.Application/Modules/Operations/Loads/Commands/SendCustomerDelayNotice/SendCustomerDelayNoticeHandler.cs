using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class SendCustomerDelayNoticeHandler(
    ITenantUnitOfWork tenantUow,
    ICustomerUpdateService customerUpdates) : IRequestHandler<SendCustomerDelayNoticeCommand, Result>
{
    public async Task<Result> Handle(SendCustomerDelayNoticeCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result.Fail($"Could not find load with ID '{req.LoadId}'", ErrorCodes.NotFound);
        }

        var result = await customerUpdates.SendDelayNoticeAsync(load, req.Message.Trim(), ct);
        await tenantUow.SaveChangesAsync(ct);
        return result;
    }
}

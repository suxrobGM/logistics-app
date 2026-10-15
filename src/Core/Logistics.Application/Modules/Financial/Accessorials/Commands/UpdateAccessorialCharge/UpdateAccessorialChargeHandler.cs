using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

internal sealed class UpdateAccessorialChargeHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<UpdateAccessorialChargeCommand, Result>
{
    public async Task<Result> Handle(UpdateAccessorialChargeCommand req, CancellationToken ct)
    {
        var repository = tenantUow.Repository<AccessorialCharge>();
        var charge = await repository.GetByIdAsync(req.Id, ct);
        if (charge is null)
        {
            return Result.Fail($"Could not find an accessorial with ID '{req.Id}'");
        }

        var name = req.Name.Trim();
        if (await repository.GetAsync(a => a.Name == name && a.Id != req.Id, ct) is not null)
        {
            return Result.Fail($"An accessorial named '{name}' already exists");
        }

        charge.Name = name;
        charge.LineItemType = req.LineItemType;
        charge.Unit = req.Unit;
        charge.DefaultAmount = req.DefaultAmount;
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

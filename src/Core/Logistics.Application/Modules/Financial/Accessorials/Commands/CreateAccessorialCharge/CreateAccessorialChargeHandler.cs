using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

internal sealed class CreateAccessorialChargeHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<CreateAccessorialChargeCommand, Result<AccessorialChargeDto>>
{
    public async Task<Result<AccessorialChargeDto>> Handle(CreateAccessorialChargeCommand req, CancellationToken ct)
    {
        var name = req.Name.Trim();
        if (await tenantUow.Repository<AccessorialCharge>().GetAsync(a => a.Name == name, ct) is not null)
        {
            return Result<AccessorialChargeDto>.Fail($"An accessorial named '{name}' already exists");
        }

        var charge = new AccessorialCharge
        {
            Name = name,
            LineItemType = req.LineItemType,
            Unit = req.Unit,
            DefaultAmount = req.DefaultAmount
        };

        await tenantUow.Repository<AccessorialCharge>().AddAsync(charge, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result<AccessorialChargeDto>.Ok(charge.ToDto());
    }
}

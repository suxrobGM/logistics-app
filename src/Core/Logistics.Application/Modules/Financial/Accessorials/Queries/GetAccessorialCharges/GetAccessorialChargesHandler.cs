using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Financial.Accessorials.Queries;

internal sealed class GetAccessorialChargesHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetAccessorialChargesQuery, Result<List<AccessorialChargeDto>>>
{
    public async Task<Result<List<AccessorialChargeDto>>> Handle(GetAccessorialChargesQuery req, CancellationToken ct)
    {
        var charges = await tenantUow.Repository<AccessorialCharge>().Query()
            .OrderBy(a => a.Name)
            .ToListAsync(ct);
        return Result<List<AccessorialChargeDto>>.Ok(charges.Select(a => a.ToDto()).ToList());
    }
}

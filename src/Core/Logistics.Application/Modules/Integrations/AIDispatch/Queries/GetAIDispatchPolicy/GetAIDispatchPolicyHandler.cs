using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

internal sealed class GetAIDispatchPolicyHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAIDispatchPolicyQuery, Result<AIDispatchPolicyDto>>
{
    public async Task<Result<AIDispatchPolicyDto>> Handle(
        GetAIDispatchPolicyQuery request, CancellationToken ct)
    {
        var policy = await tenantUow.Repository<AIDispatchPolicy>().Query().FirstOrDefaultAsync(ct);

        // No row is normal for a new tenant. A blank enabled policy keeps the null branch out of the
        // controller, the generated client and the page.
        return Result<AIDispatchPolicyDto>.Ok(policy?.ToDto() ?? AIDispatchPolicyDto.Empty);
    }
}

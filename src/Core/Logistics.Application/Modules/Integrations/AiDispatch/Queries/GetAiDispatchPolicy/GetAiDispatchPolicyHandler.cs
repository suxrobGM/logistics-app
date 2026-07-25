using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Queries;

internal sealed class GetAiDispatchPolicyHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAiDispatchPolicyQuery, Result<AiDispatchPolicyDto>>
{
    public async Task<Result<AiDispatchPolicyDto>> Handle(
        GetAiDispatchPolicyQuery request, CancellationToken ct)
    {
        var policy = await tenantUow.Repository<AiDispatchPolicy>().Query().FirstOrDefaultAsync(ct);

        // No row yet is the normal state for a new tenant, not an error. Returning a blank enabled
        // policy keeps the null branch out of the controller, the generated client and the page.
        return Result<AiDispatchPolicyDto>.Ok(policy?.ToDto() ?? AiDispatchPolicyDto.Empty);
    }
}

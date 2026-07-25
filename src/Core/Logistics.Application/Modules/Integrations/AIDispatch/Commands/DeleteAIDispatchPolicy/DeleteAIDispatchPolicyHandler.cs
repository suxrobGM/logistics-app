using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class DeleteAIDispatchPolicyHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteAIDispatchPolicyCommand, Result>
{
    public async Task<Result> Handle(DeleteAIDispatchPolicyCommand request, CancellationToken ct)
    {
        var repo = tenantUow.Repository<AIDispatchPolicy>();
        var policy = await repo.Query().FirstOrDefaultAsync(ct);

        // No row is already the state the caller asked for.
        if (policy is null)
        {
            return Result.Ok();
        }

        repo.Delete(policy);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

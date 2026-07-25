using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

internal sealed class DeleteAiDispatchPolicyHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteAiDispatchPolicyCommand, Result>
{
    public async Task<Result> Handle(DeleteAiDispatchPolicyCommand request, CancellationToken ct)
    {
        var repo = tenantUow.Repository<AiDispatchPolicy>();
        var policy = await repo.Query().FirstOrDefaultAsync(ct);

        // Deleting what does not exist is the state the caller asked for.
        if (policy is null)
        {
            return Result.Ok();
        }

        repo.Delete(policy);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

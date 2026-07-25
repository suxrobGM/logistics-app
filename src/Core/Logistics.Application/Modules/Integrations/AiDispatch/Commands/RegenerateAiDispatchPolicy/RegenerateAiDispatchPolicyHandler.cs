using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.AiDispatch.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

/// <summary>
/// Runs synchronously: one ~1k-token call on a cheap model takes a couple of seconds, so a background
/// job plus polling would add moving parts without shortening the wait.
/// </summary>
internal sealed class RegenerateAiDispatchPolicyHandler(
    IAiDispatchPolicyLearner learner,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<RegenerateAiDispatchPolicyCommand, Result<AiDispatchPolicyDto>>
{
    public async Task<Result<AiDispatchPolicyDto>> Handle(
        RegenerateAiDispatchPolicyCommand request, CancellationToken ct)
    {
        var outcome = await learner.LearnForCurrentTenantAsync(force: true, ct);

        if (!outcome.IsSuccess)
        {
            return Result<AiDispatchPolicyDto>.Fail(outcome.Error ?? "Failed to regenerate the policy.");
        }

        // Surface the skip reason (e.g. "not enough reviewed decisions yet") rather than a generic error.
        if (!outcome.Value!.Generated)
        {
            return Result<AiDispatchPolicyDto>.Fail(outcome.Value.SkipReason ?? "Nothing to learn yet.");
        }

        // A generated pass always leaves a row behind.
        var policy = await tenantUow.Repository<AiDispatchPolicy>().Query().FirstAsync(ct);

        return Result<AiDispatchPolicyDto>.Ok(policy.ToDto());
    }
}

using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.AISettings.Commands;

internal sealed class UpdateAISettingsHandler(
    ISystemSettingsService systemSettings,
    IMasterUnitOfWork masterUow) : IAppRequestHandler<UpdateAISettingsCommand, Result>
{
    public async Task<Result> Handle(UpdateAISettingsCommand req, CancellationToken ct)
    {
        var modelInfo = LlmModelCatalog.Find(req.Model);
        if (modelInfo is null)
            return Result.Fail($"Unknown AI model '{req.Model}'.");

        // Persist the global model selection (provider is derived from the model via the catalog).
        await systemSettings.SetAsync(AISettingsKeys.Model, modelInfo.Id,
            "Platform-wide AI dispatch model", ct);
        await systemSettings.SetAsync(AISettingsKeys.ExtendedThinking, req.ExtendedThinking.ToString(),
            "Whether extended thinking is enabled for the dispatch agent", ct);

        // Update per-plan weekly quotas (null = unlimited).
        var planRepo = masterUow.Repository<SubscriptionPlan>();
        var changed = false;
        foreach (var planUpdate in req.Plans)
        {
            var plan = await planRepo.GetByIdAsync(planUpdate.PlanId, ct);
            if (plan is null || plan.WeeklyAIRequestQuota == planUpdate.WeeklyAIRequestQuota)
                continue;

            plan.WeeklyAIRequestQuota = planUpdate.WeeklyAIRequestQuota;
            planRepo.Update(plan);
            changed = true;
        }

        if (changed)
            await masterUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

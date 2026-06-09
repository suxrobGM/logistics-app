using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AiDispatch;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.AiSettings.Commands;

internal sealed class UpdateAiSettingsHandler(
    ISystemSettingsService systemSettings,
    IMasterUnitOfWork masterUow) : IAppRequestHandler<UpdateAiSettingsCommand, Result>
{
    public async Task<Result> Handle(UpdateAiSettingsCommand req, CancellationToken ct)
    {
        var modelInfo = LlmModelCatalog.Find(req.Model);
        if (modelInfo is null)
            return Result.Fail($"Unknown AI model '{req.Model}'.");

        // Persist the global model selection (provider is derived from the model via the catalog).
        await systemSettings.SetAsync(AiSettingsKeys.Model, modelInfo.Id,
            "Platform-wide AI dispatch model", ct);
        await systemSettings.SetAsync(AiSettingsKeys.ExtendedThinking, req.ExtendedThinking.ToString(),
            "Whether extended thinking is enabled for the dispatch agent", ct);

        // Update per-plan weekly quotas (null = unlimited).
        var planRepo = masterUow.Repository<SubscriptionPlan>();
        var changed = false;
        foreach (var planUpdate in req.Plans)
        {
            var plan = await planRepo.GetByIdAsync(planUpdate.PlanId, ct);
            if (plan is null || plan.WeeklyAiRequestQuota == planUpdate.WeeklyAiRequestQuota)
                continue;

            plan.WeeklyAiRequestQuota = planUpdate.WeeklyAiRequestQuota;
            planRepo.Update(plan);
            changed = true;
        }

        if (changed)
            await masterUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

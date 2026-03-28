using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateTenantAiSettingsHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<UpdateTenantAiSettingsCommand, Result>
{


    /// <summary>
    /// Model-to-tier and provider mapping. Must stay in sync with LlmPricing.GetModelTier().
    /// </summary>
    private static readonly Dictionary<string, ModelInfo> Models = new()
    {
        ["deepseek-chat"] = new(LlmModelTier.Base, LlmProvider.DeepSeek),
        ["deepseek-reasoner"] = new(LlmModelTier.Base, LlmProvider.DeepSeek),
        ["gpt-5.4-mini"] = new(LlmModelTier.Base, LlmProvider.OpenAi),
        ["claude-haiku-4-5"] = new(LlmModelTier.Base, LlmProvider.Anthropic),
        ["gpt-5.4"] = new(LlmModelTier.Premium, LlmProvider.OpenAi),
        ["claude-sonnet-4-6"] = new(LlmModelTier.Premium, LlmProvider.Anthropic),
        ["claude-opus-4-6"] = new(LlmModelTier.Ultra, LlmProvider.Anthropic),
    };

    public async Task<Result> Handle(UpdateTenantAiSettingsCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var masterTenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenant.Id, ct);

        if (masterTenant is null)
            return Result.Fail("Tenant not found");

        // Validate model against plan's AllowedModelTier and infer provider
        var provider = masterTenant.Settings.LlmProvider;
        if (req.Model is not null && Models.TryGetValue(req.Model, out var info))
        {
            var allowedTier = await GetAllowedModelTierAsync(masterTenant, ct);
            if (info.Tier > allowedTier)
                return Result.Fail($"Your plan does not include access to this model. Upgrade to use {info.Tier} tier models.", ErrorCodes.ResourceLimitReached);

            provider = info.Provider;
        }

        masterTenant.Settings = masterTenant.Settings with
        {
            LlmProvider = provider,
            LlmModel = req.Model
        };

        masterUow.Repository<Tenant>().Update(masterTenant);
        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private async Task<LlmModelTier> GetAllowedModelTierAsync(Tenant tenant, CancellationToken ct)
    {
        if (tenant.Subscription is null)
            return LlmModelTier.Base;

        var plan = await masterUow.Repository<SubscriptionPlan>()
            .GetByIdAsync(tenant.Subscription.PlanId, ct);

        return plan?.AllowedModelTier ?? LlmModelTier.Base;
    }

    #region Internal records

    private record ModelInfo(LlmModelTier Tier, LlmProvider Provider);

    #endregion
}

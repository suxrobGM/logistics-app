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
    /// Model-to-tier mapping for validation. Must stay in sync with LlmPricing.GetModelTier().
    /// </summary>
    private static readonly Dictionary<string, LlmModelTier> ModelTiers = new()
    {
        ["deepseek-chat"] = LlmModelTier.Base,
        ["deepseek-reasoner"] = LlmModelTier.Base,
        ["gpt-5.4-mini"] = LlmModelTier.Base,
        ["claude-haiku-4-5"] = LlmModelTier.Base,
        ["gpt-5.4"] = LlmModelTier.Premium,
        ["claude-sonnet-4-6"] = LlmModelTier.Premium,
        ["claude-opus-4-6"] = LlmModelTier.Ultra,
    };

    public async Task<Result> Handle(UpdateTenantAiSettingsCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var masterTenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenant.Id, ct);

        if (masterTenant is null)
            return Result.Fail("Tenant not found.");

        // Validate model against plan's AllowedModelTier
        if (req.Model is not null)
        {
            var modelTier = ModelTiers.GetValueOrDefault(req.Model, LlmModelTier.Base);
            var allowedTier = await GetAllowedModelTierAsync(masterTenant, ct);

            if (modelTier > allowedTier)
                return Result.Fail($"Your plan does not include access to this model. Upgrade to use {modelTier} tier models.");
        }

        masterTenant.Settings = masterTenant.Settings with
        {
            LlmProvider = req.Provider,
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
}

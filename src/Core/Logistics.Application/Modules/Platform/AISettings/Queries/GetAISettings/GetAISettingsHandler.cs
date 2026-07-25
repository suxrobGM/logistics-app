using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.AI;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Platform.AISettings.Queries;

internal sealed class GetAISettingsHandler(
    ISystemSettingsService systemSettings,
    IMasterUnitOfWork masterUow,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<GetAISettingsQuery, Result<AISettingsDto>>
{
    public async Task<Result<AISettingsDto>> Handle(GetAISettingsQuery req, CancellationToken ct)
    {
        var config = llmOptions.Value;

        // Global model: system setting → appsettings default
        var model = await systemSettings.GetAsync(AISettingsKeys.Model, ct);
        if (string.IsNullOrWhiteSpace(model))
        {
            model = config.FindProviderModel(config.DefaultProvider);
        }

        var modelInfo = LlmModelCatalog.Find(model) ?? LlmModelCatalog.Models[0];

        // Extended thinking: system setting → appsettings default
        var thinkingSetting = await systemSettings.GetAsync(AISettingsKeys.ExtendedThinking, ct);
        var extendedThinking = bool.TryParse(thinkingSetting, out var parsedThinking)
            ? parsedThinking
            : config.EnableExtendedThinking;

        var plans = await masterUow.Repository<SubscriptionPlan>().GetListAsync(ct: ct);

        return Result<AISettingsDto>.Ok(new AISettingsDto
        {
            Model = modelInfo.Id,
            Provider = modelInfo.Provider.ToString(),
            ExtendedThinking = extendedThinking,
            AvailableModels = [.. LlmModelCatalog.Models.Select(m => new LlmModelOptionDto
            {
                Id = m.Id,
                DisplayName = m.DisplayName,
                Provider = m.Provider.ToString()
            })],
            Plans = [.. plans
                .OrderBy(p => p.Tier)
                .Select(p => new PlanQuotaDto
                {
                    PlanId = p.Id,
                    PlanName = p.Name,
                    WeeklyAIRequestQuota = p.WeeklyAIRequestQuota
                })]
        });
    }
}

using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AiDispatch;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Ai;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Platform.AiSettings.Queries;

internal sealed class GetAiSettingsHandler(
    ISystemSettingsService systemSettings,
    IMasterUnitOfWork masterUow,
    IOptions<LlmOptions> llmOptions) : IAppRequestHandler<GetAiSettingsQuery, Result<AiSettingsDto>>
{
    public async Task<Result<AiSettingsDto>> Handle(GetAiSettingsQuery req, CancellationToken ct)
    {
        var config = llmOptions.Value;

        // Global model: system setting → appsettings default
        var model = await systemSettings.GetAsync(AiSettingsKeys.Model, ct);
        if (string.IsNullOrWhiteSpace(model))
        {
            model = config.FindProviderModel(config.DefaultProvider);
        }

        var modelInfo = LlmModelCatalog.Find(model) ?? LlmModelCatalog.Models[0];

        // Extended thinking: system setting → appsettings default
        var thinkingSetting = await systemSettings.GetAsync(AiSettingsKeys.ExtendedThinking, ct);
        var extendedThinking = bool.TryParse(thinkingSetting, out var parsedThinking)
            ? parsedThinking
            : config.EnableExtendedThinking;

        var plans = await masterUow.Repository<SubscriptionPlan>().GetListAsync(ct: ct);

        return Result<AiSettingsDto>.Ok(new AiSettingsDto
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
                    WeeklyAiRequestQuota = p.WeeklyAiRequestQuota
                })]
        });
    }
}

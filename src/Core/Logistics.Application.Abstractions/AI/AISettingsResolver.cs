using Logistics.Application.Abstractions.Context;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AI;

/// <summary>Shared by the agents and the AI Settings page so both resolve a setting the same way.</summary>
public static class AISettingsResolver
{
    extension(ISystemSettingsService systemSettings)
    {
        /// <summary>The admin's setting, falling back to <see cref="LlmOptions.DefaultReasoningEffort"/>.</summary>
        public async Task<ReasoningEffort> ResolveReasoningEffortAsync(
            LlmOptions config,
            CancellationToken ct = default)
        {
            var setting = await systemSettings.GetAsync(AISettingsKeys.ReasoningEffort, ct);
            return Enum.TryParse<ReasoningEffort>(setting, out var effort)
                ? effort
                : config.DefaultReasoningEffort;
        }
    }
}

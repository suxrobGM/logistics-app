using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AI;

/// <summary>
/// Reads the admin-managed AI settings that both the runtime and the admin screen need. Lives here
/// rather than beside either caller so the two cannot resolve the same setting differently - the
/// AI Settings page would then display an effort the agents are not using.
/// </summary>
public static class AISettingsResolver
{
    /// <summary>
    /// Global reasoning effort: system setting (<see cref="AISettingsKeys.ReasoningEffort"/>) →
    /// <see cref="LlmOptions.DefaultReasoningEffort"/> from appsettings.
    /// </summary>
    public static async Task<ReasoningEffort> ResolveReasoningEffortAsync(
        this ISystemSettingsService systemSettings,
        LlmOptions config,
        CancellationToken ct = default)
    {
        var setting = await systemSettings.GetAsync(AISettingsKeys.ReasoningEffort, ct);
        return Enum.TryParse<ReasoningEffort>(setting, out var effort)
            ? effort
            : config.DefaultReasoningEffort;
    }
}

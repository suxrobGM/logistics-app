using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// Platform-wide AI dispatch settings managed by an admin: the global model and per-plan
/// weekly budgets. Internal model names are admin-only and never shown to tenants.
/// </summary>
public record AISettingsDto
{
    /// <summary>The globally selected model id (e.g. "gpt-5.6-luna").</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>The resolved provider for the selected model.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Global reasoning depth for both agents. Ignored by models without a reasoning control.
    /// </summary>
    public ReasoningEffort ReasoningEffort { get; set; }

    /// <summary>All models an admin can choose from.</summary>
    public List<LlmModelOptionDto> AvailableModels { get; set; } = [];

    /// <summary>Editable weekly AI budgets per subscription plan.</summary>
    public List<PlanQuotaDto> Plans { get; set; } = [];
}

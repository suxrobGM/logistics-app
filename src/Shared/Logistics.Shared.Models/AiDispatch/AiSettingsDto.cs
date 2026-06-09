namespace Logistics.Shared.Models;

/// <summary>
/// Platform-wide AI dispatch settings managed by an admin: the global model and per-plan
/// weekly quotas. Internal model names are admin-only and never shown to tenants.
/// </summary>
public record AiSettingsDto
{
    /// <summary>The globally selected model id (e.g. "deepseek-v4-flash").</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>The resolved provider for the selected model.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Whether extended thinking is enabled. Only honored by models that support it.
    /// </summary>
    public bool ExtendedThinking { get; set; }

    /// <summary>All models an admin can choose from.</summary>
    public List<LlmModelOptionDto> AvailableModels { get; set; } = [];

    /// <summary>Editable weekly AI quotas per subscription plan.</summary>
    public List<PlanQuotaDto> Plans { get; set; } = [];
}

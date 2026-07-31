using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.AISettings.Commands;

/// <summary>
/// Sets the platform-wide AI dispatch model and per-plan weekly budgets. Admin only.
/// </summary>
public sealed class UpdateAISettingsCommand : ICommand
{
    /// <summary>The globally selected model id (must exist in the model catalog).</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Global reasoning depth. Ignored by models without a reasoning control.</summary>
    public ReasoningEffort ReasoningEffort { get; set; }

    /// <summary>Per-plan weekly budget updates. A null budget means unlimited.</summary>
    public List<PlanQuotaDto> Plans { get; set; } = [];
}

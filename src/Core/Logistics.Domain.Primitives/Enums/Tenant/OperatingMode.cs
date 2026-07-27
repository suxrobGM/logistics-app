using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>How the tenant runs its business - shapes the UX preset and the AI dispatch prompt.</summary>
public enum OperatingMode
{
    /// <summary>A carrier with a back office - separate dispatchers, drivers and payroll.</summary>
    Fleet,

    /// <summary>A one-person carrier who is owner, dispatcher and driver at once.</summary>
    [Description("Owner-Operator (Solo)")]
    SoloOperator
}

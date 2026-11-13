namespace Logistics.Domain.Primitives.Enums;

/// <summary>How much reasoning the model is asked to do. Each LLM provider maps it to its own setting and uses the nearest supported level when it lacks this one.</summary>
public enum ReasoningEffort
{
    None,
    Low,
    Medium,
    High,
    XHigh,
    Max
}

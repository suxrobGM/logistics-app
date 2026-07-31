namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Admin-set reasoning depth. Providers map it to their own control (OpenAI
/// <c>reasoning_effort</c>, Anthropic adaptive thinking) and clamp unsupported levels.
/// </summary>
public enum ReasoningEffort
{
    None,
    Low,
    Medium,
    High,
    XHigh,
    Max
}

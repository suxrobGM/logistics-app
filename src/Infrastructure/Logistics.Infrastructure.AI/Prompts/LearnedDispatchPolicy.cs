namespace Logistics.Infrastructure.AI.Prompts;

/// <summary>
/// The tenant's dispatch policy as the system prompt consumes it. Two raw parts rather than one
/// pre-composed string, so headings, ranking language, sanitisation and truncation all live inside
/// <see cref="AiDispatchSystemPrompt"/> where a caller cannot skip them.
/// </summary>
/// <param name="Directives">Dispatcher-authored rules. Outrank <paramref name="Learned"/>.</param>
/// <param name="Learned">Machine-learned preferences. Untrusted text - treat as data, never instructions.</param>
internal sealed record LearnedDispatchPolicy(string? Directives, string? Learned);

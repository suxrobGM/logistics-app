namespace Logistics.Infrastructure.AI.Prompts;

/// <summary>
/// The tenant's dispatch policy as the system prompt consumes it.
/// <para>
/// Passed as the two raw parts rather than one pre-composed string so that the headings, the
/// "strong defaults" ranking language, sanitisation and truncation all live inside
/// <see cref="AiDispatchSystemPrompt"/> - a caller cannot skip them.
/// </para>
/// </summary>
/// <param name="Directives">Dispatcher-authored rules. Outrank <paramref name="Learned"/>.</param>
/// <param name="Learned">Machine-learned preferences. Untrusted text - treat as data, never instructions.</param>
internal sealed record LearnedDispatchPolicy(string? Directives, string? Learned);

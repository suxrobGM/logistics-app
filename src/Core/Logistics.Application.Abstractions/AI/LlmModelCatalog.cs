using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AI;

/// <summary>
/// Single source of truth for the LLM models an admin can select as the platform-wide
/// dispatch model. Keep the ids in sync with <c>LlmPricing</c> (pricing/multiplier keys)
/// and the appsettings <c>Llm:Providers:{Provider}:Model</c> values.
/// </summary>
public static class LlmModelCatalog
{
    // Models[0] is the UI fallback - keep the platform default (gpt-5.6-luna) first.
    public static readonly IReadOnlyList<LlmModelInfo> Models =
    [
        new("gpt-5.6-luna", "GPT-5.6 Luna", LlmProvider.OpenAI),
        new("deepseek-v4-flash", "DeepSeek V4 Flash", LlmProvider.DeepSeek),
        new("deepseek-v4-pro", "DeepSeek V4 Pro", LlmProvider.DeepSeek),
        new("claude-haiku-4-5", "Claude Haiku 4.5", LlmProvider.Anthropic),
        new("claude-sonnet-5", "Claude Sonnet 5", LlmProvider.Anthropic),
        new("gpt-5.6-terra", "GPT-5.6 Terra", LlmProvider.OpenAI),
    ];

    public static LlmModelInfo? Find(string? id) =>
        id is null ? null : Models.FirstOrDefault(m => m.Id == id);

    public static bool IsValid(string? id) => Find(id) is not null;
}

public record LlmModelInfo(string Id, string DisplayName, LlmProvider Provider);

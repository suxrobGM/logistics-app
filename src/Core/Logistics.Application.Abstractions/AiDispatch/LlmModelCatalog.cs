using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AiDispatch;

/// <summary>
/// Single source of truth for the LLM models an admin can select as the platform-wide
/// dispatch model. Keep the ids in sync with <c>LlmPricing</c> (pricing/multiplier keys)
/// and the appsettings <c>Llm:Providers:{Provider}:Model</c> values.
/// </summary>
public static class LlmModelCatalog
{
    public static readonly IReadOnlyList<LlmModelInfo> Models =
    [
        new("deepseek-v4-flash", "DeepSeek V4 Flash", LlmProvider.DeepSeek),
        new("deepseek-v4-pro", "DeepSeek V4 Pro", LlmProvider.DeepSeek),
        new("gpt-5.4-mini", "GPT-5.4 Mini", LlmProvider.OpenAi),
        new("gpt-5.4", "GPT-5.4", LlmProvider.OpenAi),
        new("claude-haiku-4-5", "Claude Haiku 4.5", LlmProvider.Anthropic),
        new("claude-sonnet-4-6", "Claude Sonnet 4.6", LlmProvider.Anthropic),
        new("claude-opus-4-8", "Claude Opus 4.8", LlmProvider.Anthropic),
    ];

    public static LlmModelInfo? Find(string? id) =>
        id is null ? null : Models.FirstOrDefault(m => m.Id == id);

    public static bool IsValid(string? id) => Find(id) is not null;
}

public record LlmModelInfo(string Id, string DisplayName, LlmProvider Provider);

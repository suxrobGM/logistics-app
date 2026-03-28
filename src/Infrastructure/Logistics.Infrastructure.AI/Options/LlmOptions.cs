namespace Logistics.Infrastructure.AI.Options;

public class LlmOptions
{
    public const string SectionName = "Llm";

    public static readonly string[] AllowedModels =
        ["claude-sonnet-4-6", "claude-haiku-4-5", "claude-opus-4-6"];

    public required string ApiKey { get; set; }
    public string Model { get; set; } = "claude-sonnet-4-6";
    public int MaxTokens { get; set; } = 8192;
    public bool EnableExtendedThinking { get; set; }
    public int ThinkingBudgetTokens { get; set; } = 10000;
}

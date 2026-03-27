namespace Logistics.Infrastructure.AI.Options;

public class ClaudeOptions
{
    public const string SectionName = "Claude";

    public required string ApiKey { get; set; }
    public string Model { get; set; } = "claude-sonnet-4-6";
    public int MaxTokens { get; set; } = 8192;
}

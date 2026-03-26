namespace Logistics.Infrastructure.AI.Options;

public class ClaudeOptions
{
    public const string SectionName = "Claude";

    public required string ApiKey { get; set; }
    public string Model { get; set; } = "claude-haiku-4-5";
    public int MaxTokens { get; set; } = 4096;
}

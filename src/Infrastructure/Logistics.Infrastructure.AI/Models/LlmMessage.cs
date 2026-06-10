namespace Logistics.Infrastructure.AI.Models;

internal enum LlmRole
{
    User,
    Assistant
}

/// <summary>
/// A single conversation turn: a role plus its ordered content blocks.
/// </summary>
internal record LlmMessage(LlmRole Role, List<LlmContentBlock> Content)
{
    public static LlmMessage FromUser(string text) =>
        new(LlmRole.User, [new LlmTextBlock(text)]);

    public static LlmMessage FromToolResults(List<LlmToolResultBlock> results) =>
        new(LlmRole.User, [.. results]);
}

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Who authored a copilot transcript message. System rows are app-generated notes
/// (approval/rejection outcomes) that the next turn replays as context.
/// </summary>
public enum AICopilotMessageRole
{
    User,
    Assistant,
    System
}

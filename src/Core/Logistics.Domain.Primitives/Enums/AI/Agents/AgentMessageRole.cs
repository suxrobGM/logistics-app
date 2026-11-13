namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Who authored an agent transcript message. System rows are app-generated notes
/// (approval/rejection outcomes) the next turn replays as context.
/// </summary>
public enum AgentMessageRole
{
    User,
    Assistant,
    System
}

namespace Logistics.Domain.Primitives.Enums;

/// <summary>Author of an agent message. System messages are app-generated notes (e.g. approval outcomes) included as context in the next turn.</summary>
public enum AgentMessageRole
{
    User,
    Assistant,
    System
}

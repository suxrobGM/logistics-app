using Logistics.Application.Abstractions.Agents;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// A tool's catalogue entry. Static because the catalogue is read at startup, before a tool's
/// scoped dependencies exist.
/// </summary>
internal interface IAgentToolMetadata
{
    static abstract AgentToolDefinition Definition { get; }
}

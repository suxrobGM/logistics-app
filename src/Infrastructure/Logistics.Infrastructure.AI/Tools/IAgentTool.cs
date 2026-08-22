using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>Implement through <see cref="AgentTool{TInput}"/>, so arguments arrive typed.</summary>
internal interface IAgentTool
{
    Task<string> ExecuteAsync(JsonNode input, CancellationToken ct);
}

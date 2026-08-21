using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Base for every agent tool. <typeparamref name="TInput"/> is the one declaration of what the tool
/// accepts: its schema is exported from it, and the model's reply is bound back into it.
/// </summary>
internal abstract class AgentTool<TInput> : IAgentTool where TInput : class
{
    protected abstract Task<string> ExecuteAsync(TInput input, CancellationToken ct);

    public Task<string> ExecuteAsync(JsonNode input, CancellationToken ct) =>
        AgentToolJson.TryBind<TInput>(input, out var typed, out var error)
            ? ExecuteAsync(typed, ct)
            : Task.FromResult(ToolResult.Error(error));
}

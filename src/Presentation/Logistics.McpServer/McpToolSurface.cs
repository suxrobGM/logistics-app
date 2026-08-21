using System.Collections.Concurrent;
using System.Text.Json;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using ModelContextProtocol.Protocol;

namespace Logistics.McpServer;

/// <summary>
/// Serves the tool catalogue and tool calls over MCP from the shared <see cref="IAgentToolRegistry"/>.
/// Scoped, because the catalogue depends on the calling tenant's features.
/// </summary>
internal sealed class McpToolSurface(
    IAgentToolRegistry registry,
    IAgentToolExecutor executor,
    IFeatureService featureService,
    ITenantUnitOfWork tenantUow)
{
    /// <summary>
    /// The catalogue is fixed at startup, so each entry converts once. Rebuilding it per request
    /// re-serialized and reparsed every tool's schema to reach <see cref="JsonElement"/>.
    /// </summary>
    private static readonly ConcurrentDictionary<string, Tool> ProtocolTools = new();

    public async ValueTask<ListToolsResult> ListToolsAsync(CancellationToken ct)
    {
        var tools = registry.GetMcpTools(await EnabledFeaturesAsync());

        return new ListToolsResult { Tools = [.. tools.Select(ToProtocolTool)] };
    }

    public async ValueTask<CallToolResult> CallToolAsync(CallToolRequestParams? request, CancellationToken ct)
    {
        if (request?.Name is not { Length: > 0 } name || registry.TryGetDefinition(name) is not { } definition)
            return Error($"Unknown tool: {request?.Name}");

        // The catalogue hides these, but a client can call any name it likes.
        if (definition.RequiresHumanOrigin)
        {
            return Error(
                $"The {name} tool is not available over MCP: it records the person responsible for "
                + "the action, and an API key identifies a tenant rather than a user.");
        }

        if (definition.RequiredFeature is { } feature &&
            !await featureService.IsFeatureEnabledAsync(TenantId, feature))
        {
            return Error($"The {feature.GetDescription()} feature is not enabled for this tenant.");
        }

        var arguments = request.Arguments is { } args ? JsonSerializer.Serialize(args) : "{}";
        var result = await executor.ExecuteToolAsync(name, arguments, ct);

        return new CallToolResult { Content = [new TextContentBlock { Text = result }] };
    }

    private Guid TenantId => tenantUow.GetCurrentTenant().Id;

    private async Task<IReadOnlySet<TenantFeature>> EnabledFeaturesAsync() =>
        (await featureService.GetEnabledFeaturesAsync(TenantId)).ToHashSet();

    private static Tool ToProtocolTool(AgentToolDefinition definition) =>
        ProtocolTools.GetOrAdd(definition.Name, _ => new Tool
        {
            Name = definition.Name,
            Description = definition.Description,
            InputSchema = definition.InputSchema.Deserialize<JsonElement>(),
            Annotations = new ToolAnnotations
            {
                ReadOnlyHint = !definition.IsWrite,
                // Writes create or send; nothing overwrites or deletes what the client did not name.
                DestructiveHint = false,
                OpenWorldHint = false
            }
        });

    private static CallToolResult Error(string message) => new()
    {
        IsError = true,
        Content = [new TextContentBlock { Text = JsonSerializer.Serialize(new { error = message }) }]
    };
}

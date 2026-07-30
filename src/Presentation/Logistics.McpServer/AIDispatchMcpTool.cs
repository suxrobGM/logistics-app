using System.Text.Json;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.McpServer;

/// <summary>
/// An MCP tool that wraps a <see cref="AgentToolDefinition"/> and delegates execution
/// to <see cref="IAgentToolExecutor"/>. Generated dynamically from the tool registry
/// so tool names, descriptions, and schemas are defined in one place.
/// </summary>
internal sealed class AIDispatchMcpTool : McpServerTool
{
    private const string WriteWarning =
        " ⚠️ WRITE OPERATION: Always explain what you're about to do and get explicit user confirmation before calling this tool.";

    private readonly Tool protocolTool;
    private readonly TenantFeature? requiredFeature;

    public AIDispatchMcpTool(AgentToolDefinition definition)
    {
        requiredFeature = definition.RequiredFeature;

        var description = definition.IsWrite
            ? definition.Description + WriteWarning
            : definition.Description;

        var inputSchema = definition.InputSchema.Deserialize<JsonElement>();

        protocolTool = new Tool
        {
            Name = definition.Name,
            Description = description,
            InputSchema = inputSchema
        };
    }

    public override Tool ProtocolTool => protocolTool;
    public override IReadOnlyList<object> Metadata => [];

    public override async ValueTask<CallToolResult> InvokeAsync(
        RequestContext<CallToolRequestParams> request,
        CancellationToken cancellationToken)
    {
        var services = request.Services!;
        var featureService = services.GetRequiredService<IFeatureService>();
        var tenantUow = services.GetRequiredService<ITenantUnitOfWork>();
        var executor = services.GetRequiredService<IAgentToolExecutor>();

        // Feature gate: MCP Server
        var tenant = tenantUow.GetCurrentTenant();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.McpServer))
        {
            return ErrorResult("MCP Server feature is not enabled for this tenant. Please upgrade your subscription plan.");
        }

        // Feature gate: whatever the tool itself declares. The agent path filters gated tools out of
        // the catalogue, but MCP publishes every tool at startup - so this is where that gate holds.
        if (requiredFeature is { } feature &&
            !await featureService.IsFeatureEnabledAsync(tenant.Id, feature))
        {
            return ErrorResult($"The {feature.GetDescription()} feature is not enabled for this tenant.");
        }

        // Serialize arguments to JSON and delegate to the tool executor
        var inputJson = request.Params?.Arguments is { } args
            ? JsonSerializer.Serialize(args)
            : "{}";

        var result = await executor.ExecuteToolAsync(protocolTool.Name, inputJson, cancellationToken);

        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = result }]
        };
    }

    private static CallToolResult ErrorResult(string message)
    {
        var errorJson = JsonSerializer.Serialize(new { error = message });
        return new CallToolResult
        {
            IsError = true,
            Content = [new TextContentBlock { Text = errorJson }]
        };
    }
}

using System.Text.Json;
using Logistics.Application.Services;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Logistics.McpServer;

/// <summary>
/// An MCP tool that wraps a <see cref="DispatchToolDefinition"/> and delegates execution
/// to <see cref="IDispatchToolExecutor"/>. Generated dynamically from the tool registry
/// so tool names, descriptions, and schemas are defined in one place.
/// </summary>
internal sealed class DispatchMcpTool : McpServerTool
{
    private const string WriteWarning =
        " ⚠️ WRITE OPERATION: Always explain what you're about to do and get explicit user confirmation before calling this tool.";

    private static readonly HashSet<string> WriteTools =
        ["assign_load_to_truck", "create_trip", "dispatch_trip", "book_loadboard_load"];

    private static readonly HashSet<string> LoadBoardTools =
        ["search_loadboard", "book_loadboard_load"];

    private readonly Tool protocolTool;

    public DispatchMcpTool(DispatchToolDefinition definition)
    {
        var description = WriteTools.Contains(definition.Name)
            ? definition.Description + WriteWarning
            : definition.Description;

        var inputSchema = JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(definition.InputSchema));

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
        var tenantService = services.GetRequiredService<ITenantService>();
        var executor = services.GetRequiredService<IDispatchToolExecutor>();

        // Feature gate: MCP Server
        var tenant = tenantService.GetCurrentTenant();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.McpServer))
        {
            return ErrorResult("MCP Server feature is not enabled for this tenant. Please upgrade your subscription plan.");
        }

        // Feature gate: Load Board (for load board tools only)
        if (LoadBoardTools.Contains(protocolTool.Name) &&
            !await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.LoadBoard))
        {
            return ErrorResult("Load Board feature is not enabled for this tenant.");
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

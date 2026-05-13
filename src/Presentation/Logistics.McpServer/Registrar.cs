using System.Security.Claims;
using System.Threading.RateLimiting;
using Logistics.McpServer.Authentication;
using ModelContextProtocol.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.McpServer;

public static class Registrar
{
    public static IServiceCollection AddMcpServerInfrastructure(this IServiceCollection services)
    {
        // Build MCP tools dynamically from the dispatch tool registry (single source of truth)
        var registry = services.BuildServiceProvider().GetRequiredService<IAiDispatchToolRegistry>();
        var mcpTools = registry.GetToolDefinitions(includeLoadBoardTools: true)
            .Select(def => (McpServerTool)new AiDispatchMcpTool(def))
            .ToList();

        // MCP server with Streamable HTTP transport and server instructions
        services.AddMcpServer()
            .WithHttpTransport()
            .WithTools(mcpTools);

        services.Configure<McpServerOptions>(options =>
            options.ServerInstructions = McpServerInstructions.Text);

        // API key authentication scheme
        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyDefaults.AuthenticationScheme, _ => { });

        // MCP-specific authorization policy (API key scheme only)
        services.AddAuthorizationBuilder()
            .AddPolicy("mcp", policy =>
            {
                policy.AddAuthenticationSchemes(ApiKeyDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });

        // MCP rate limit policy (100 requests/min per API key)
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("mcp", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? context.Connection.RemoteIpAddress?.ToString()
                                  ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    public static WebApplication MapMcpEndpoint(this WebApplication app)
    {
        app.MapMcp("/mcp")
            .RequireAuthorization("mcp")
            .RequireRateLimiting("mcp");
        return app;
    }
}

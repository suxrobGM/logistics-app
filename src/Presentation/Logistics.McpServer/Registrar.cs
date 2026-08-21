using Logistics.Application.Abstractions.Agents;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Logistics.McpServer.Authentication;
using ModelContextProtocol.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.McpServer;

public static class Registrar
{
    public static IServiceCollection AddMcpServerInfrastructure(this IServiceCollection services)
    {
        // Stateless is v2's default; pinned so an SDK default change can't silently re-add
        // per-session transport state this API-key-per-request server never needed.
        services.AddMcpServer()
            .WithHttpTransport(options => options.Stateless = true);

        // Tools and instructions come from the shared registry at options-build time: no interim
        // service provider at registration, no ordering dependency on AddAIInfrastructure.
        services.AddOptions<McpServerOptions>()
            .Configure<IAgentToolRegistry>((options, registry) =>
            {
                options.ServerInstructions = McpServerInstructions.Text;

                options.ToolCollection ??= [];
                // No tenant context at startup, so list every tool - AIDispatchMcpTool gates per call.
                foreach (var definition in registry.GetAllTools())
                {
                    options.ToolCollection.Add(new AIDispatchMcpTool(definition));
                }
            });

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

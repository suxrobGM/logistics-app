using System.Security.Claims;
using System.Threading.RateLimiting;
using Logistics.McpServer.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.McpServer;

public static class Registrar
{
    public static IServiceCollection AddMcpServerInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<McpToolSurface>();

        // Stateless is v2's default; pinned so an SDK default change can't silently re-add
        // per-session transport state this API-key-per-request server never needed.
        services.AddMcpServer(options => options.ServerInstructions = McpServerInstructions.Text)
            .WithHttpTransport(options => options.Stateless = true)
            // No tool collection: the catalogue depends on the calling tenant's features, which
            // are only known per request.
            .WithListToolsHandler((request, ct) =>
                request.Services!.GetRequiredService<McpToolSurface>().ListToolsAsync(ct))
            .WithCallToolHandler((request, ct) =>
                request.Services!.GetRequiredService<McpToolSurface>().CallToolAsync(request.Params, ct));

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
        // Grouped only so the feature gate can be an endpoint filter: MapMcp returns a plain
        // convention builder, which takes no filters.
        var mcp = app.MapGroup("")
            .RequireAuthorization("mcp")
            .RequireRateLimiting("mcp")
            .AddEndpointFilter<McpFeatureGate>();

        mcp.MapMcp("/mcp");
        return app;
    }
}

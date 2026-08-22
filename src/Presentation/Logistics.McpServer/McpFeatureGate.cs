using System.Text.Json;
using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.AspNetCore.Http;

namespace Logistics.McpServer;

/// <summary>
/// Refuses the endpoint when the tenant's plan does not include the MCP server. One check covers
/// initialising, listing and calling, instead of a working handshake followed by errors.
/// </summary>
internal sealed class McpFeatureGate(IFeatureService featureService, ITenantUnitOfWork tenantUow)
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var tenant = tenantUow.GetCurrentTenant();

        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.McpServer))
        {
            return Results.Json(
                new { error = "MCP Server feature is not enabled for this tenant. Please upgrade your subscription plan." },
                statusCode: StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}

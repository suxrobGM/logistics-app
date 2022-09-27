#nullable enable
namespace Logistics.IdentityServer.Extensions;

public static class HttpContextExtensions
{
    public static string? GetTenantId(this HttpContext context)
    {
        context.Request.Headers.TryGetValue("X-Tenant", out var tenantId);
        return tenantId;
    }
}
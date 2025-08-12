#nullable enable
namespace Logistics.IdentityServer.Extensions;

public static class HttpContextExtensions
{
    public static string? GetTenantId(this HttpContext context)
    {
        context.Request.Headers.TryGetValue("x-tenant", out var tenantId);
        return tenantId;
    }
}

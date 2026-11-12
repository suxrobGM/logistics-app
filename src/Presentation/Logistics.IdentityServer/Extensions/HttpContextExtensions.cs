namespace Logistics.IdentityServer.Extensions;

public static class HttpContextExtensions
{
    extension(HttpContext context)
    {
        public Guid? GetTenantId()
        {
            context.Request.Headers.TryGetValue("x-tenant", out var tenantId);
            return Guid.TryParse(tenantId, out var id) ? id : null;
        }
    }
}

namespace Logistics.OfficeApp;

public class MultitenancyMiddleware
{
    private readonly RequestDelegate _next;

    public MultitenancyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IApiClient apiClient)
    {
        var tenantSubDomain = GetSubDomain(context.Request.Host);
        var tenantQuery = context.Request.Query["tenant"].ToString();
        var tenantCookie = context.Request.Cookies["X-Tenant"];
        var tenantId = string.Empty;

        if (!string.IsNullOrEmpty(tenantSubDomain))
        {
            tenantId = tenantSubDomain.ToLower();
        }
        else if (!string.IsNullOrEmpty(tenantQuery))
        {
            tenantId = tenantQuery.ToLower();
        }
        else if (!string.IsNullOrEmpty(tenantCookie))
        {
            tenantId = tenantCookie.ToLower();
        }

        if (!string.IsNullOrEmpty(tenantId))
        {
            apiClient.SetCurrentTenantId(tenantId);
            SetTenantCookie(context, tenantId);
        }
        
        await _next(context);
    }

    private string GetSubDomain(HostString hostString)
    {
        var subDomain = string.Empty;
        var domains = hostString.Host.Split('.');

        if (domains.Length <= 2)
            return subDomain;

        subDomain = domains[0];
        return subDomain;
    }

    private void SetTenantCookie(HttpContext context, string tenantId)
    {
        var cookieOptions = new CookieOptions
        {
            IsEssential = true,
            Secure = true
        };
        context.Response.Cookies.Append("X-Tenant", tenantId, cookieOptions);
    }
}

public static class MultitenancyMiddlewareExtensions
{
    public static IApplicationBuilder UseMultitenancy(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MultitenancyMiddleware>();
    }
}
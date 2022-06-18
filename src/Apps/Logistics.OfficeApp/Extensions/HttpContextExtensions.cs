namespace Logistics.OfficeApp;

public static class HttpContextExtensions
{
    public static string GetTenantId(this HttpContext context)
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
        
        return tenantId;
    }
    
    public static void SetTenantId(this HttpContext context, string? tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            return;
        }
        
        var cookieOptions = new CookieOptions
        {
            IsEssential = true,
            Secure = true
        };
        context.Response.Cookies.Append("X-Tenant", tenantId, cookieOptions);
    }
    
    private static string GetSubDomain(HostString hostString)
    {
        var subDomain = string.Empty;
        var domains = hostString.Host.Split('.');

        if (domains.Length <= 2)
            return subDomain;

        subDomain = domains[0];
        return subDomain;
    }
}
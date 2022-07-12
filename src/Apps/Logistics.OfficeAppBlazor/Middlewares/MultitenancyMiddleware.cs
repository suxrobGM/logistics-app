namespace Logistics.OfficeApp;

public class MultitenancyMiddleware
{
    private readonly RequestDelegate _next;

    public MultitenancyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.GetTenantId();
        context.SetTenantId(tenantId);
        await _next(context);
    }
}

public static class MultitenancyMiddlewareExtensions
{
    public static IApplicationBuilder UseMultitenancy(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MultitenancyMiddleware>();
    }
}
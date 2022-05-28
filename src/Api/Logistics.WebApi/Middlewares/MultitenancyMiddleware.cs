namespace Logistics.WebApi.Middlewares;

public class MultitenancyMiddleware
{
    private readonly RequestDelegate _next;

    public MultitenancyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        foreach (var header in context.Request.Headers)
        {
            Console.WriteLine($"{header.Key} - {header.Value}");
        }
        var a = context.Request.Headers["X-TenantId"].FirstOrDefault();
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
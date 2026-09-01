using Logistics.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Logistics.HostDefaults;

/// <summary>
/// Stamps every response with the product version so deployed instances can be identified.
/// Register it first in the pipeline so short-circuited responses (401, 429) carry it too.
/// </summary>
public static class ProductVersionHeaderExtensions
{
    public const string HeaderName = "X-LogisticsX-Version";

    public static WebApplication UseLogisticsProductVersionHeader(this WebApplication app)
    {
        app.Use((context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = ProductInfo.Version;
                return Task.CompletedTask;
            });
            return next(context);
        });

        return app;
    }
}

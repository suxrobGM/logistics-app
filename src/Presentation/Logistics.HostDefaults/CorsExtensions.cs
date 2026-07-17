using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Logistics.HostDefaults;

/// <summary>
/// Shared CORS wiring for the presentation hosts: a wildcard-subdomain "DefaultCors" policy for
/// production plus a permissive "AnyCors" policy used only in development.
/// </summary>
public static class CorsExtensions
{
    public const string DefaultCorsPolicy = "DefaultCors";
    public const string AnyCorsPolicy = "AnyCors";

    public static IServiceCollection AddLogisticsCors(
        this IServiceCollection services, params string[] productionOrigins)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicy, cors =>
            {
                cors.SetIsOriginAllowedToAllowWildcardSubdomains()
                    .WithOrigins(productionOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

            options.AddPolicy(AnyCorsPolicy, cors =>
            {
                cors.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static WebApplication UseLogisticsCors(this WebApplication app)
    {
        app.UseCors(app.Environment.IsDevelopment() ? AnyCorsPolicy : DefaultCorsPolicy);
        return app;
    }
}

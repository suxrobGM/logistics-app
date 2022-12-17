using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Sdk.Implementations;
using Logistics.Sdk.Options;

namespace Logistics.Sdk;

public static class Registrar
{
    public static IServiceCollection AddWebApiClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "ApiClient")
    {
        var options = configuration.GetSection(sectionName).Get<ApiClientOptions>();
        if (options != null)
        {
            services.AddWebApiClient(options);
        }

        return services;
    }

    public static IServiceCollection AddWebApiClient(
        this IServiceCollection services,
        Action<ApiClientOptions> configure)
    {
        var options = new ApiClientOptions();
        configure(options);
        return services.AddWebApiClient(options);
    }

    public static IServiceCollection AddWebApiClient(
        this IServiceCollection services,
        ApiClientOptions options)
    {
        services.AddSingleton(options);
        services.AddScoped<IApiClient, ApiClient>();
        return services;
    }
}
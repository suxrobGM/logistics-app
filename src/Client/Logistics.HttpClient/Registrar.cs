using Logistics.HttpClient.Implementations;
using Logistics.HttpClient.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.HttpClient;

public static class Registrar
{
    public static IServiceCollection AddApiHttpClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "ApiClient")
    {
        var options = configuration.GetSection(sectionName).Get<ApiClientOptions>();
        if (options != null)
        {
            services.AddApiHttpClient(options);
        }

        return services;
    }

    public static IServiceCollection AddApiHttpClient(
        this IServiceCollection services,
        Action<ApiClientOptions> configure)
    {
        var options = new ApiClientOptions();
        configure(options);
        return services.AddApiHttpClient(options);
    }

    public static IServiceCollection AddApiHttpClient(
        this IServiceCollection services,
        ApiClientOptions options)
    {
        services.AddSingleton(options);
        services.AddScoped<IApiClient, ApiClient>();
        return services;
    }
}

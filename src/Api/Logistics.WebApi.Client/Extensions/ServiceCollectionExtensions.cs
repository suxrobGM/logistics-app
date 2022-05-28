using Logistics.WebApi.Client.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.WebApi.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApiClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "ApiClient")
    {
        var options = configuration.GetSection(sectionName).Get<ApiClientOptions>();
        return AddWebApiClient(services, options);
    }

    public static IServiceCollection AddWebApiClient(
        this IServiceCollection services,
        Action<ApiClientOptions> configure)
    {
        var options = new ApiClientOptions();
        configure(options);
        return AddWebApiClient(services, options);
    }

    public static IServiceCollection AddWebApiClient(
        this IServiceCollection services,
        ApiClientOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<IApiClient, ApiClient>();
        return services;
    }
}
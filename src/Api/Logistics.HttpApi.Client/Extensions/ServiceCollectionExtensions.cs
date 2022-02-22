using Microsoft.Extensions.DependencyInjection;

namespace Logistics.WebApi.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApiClient(
        this IServiceCollection services)
    {
        services.AddScoped<IApiClient, ApiClient>();
        return services;
    }
}
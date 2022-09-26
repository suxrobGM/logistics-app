using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenantApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
        return services;
    }
}
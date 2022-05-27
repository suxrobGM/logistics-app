using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Mappers;

namespace Logistics.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMainApplicationLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(o =>
        {
            o.AddProfile<TenantProfile>();
        });

        services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
        return services;
    }
}
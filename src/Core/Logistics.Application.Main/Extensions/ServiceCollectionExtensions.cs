using Logistics.Application.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services)
    {
        services.AddAutoMapper(o =>
        {
            o.AddProfile<TenantProfile>();
        });

        
        services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
        return services;
    }
}
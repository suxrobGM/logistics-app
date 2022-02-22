using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Mappers;

namespace Logistics.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services)
    {
        services.AddAutoMapper(o =>
        {
            o.AddProfile<UserProfile>();
        });

        services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
        return services;
    }
}
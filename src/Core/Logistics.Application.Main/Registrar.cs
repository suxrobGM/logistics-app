using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application;

public static class Registrar
{
    public static IServiceCollection AddMainApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(typeof(Registrar).Assembly);
        return services;
    }
}
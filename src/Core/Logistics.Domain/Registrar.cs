using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Domain;

public static class Registrar
{
    public static IServiceCollection AddDomainLayer(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly));
        return services;
    }
}
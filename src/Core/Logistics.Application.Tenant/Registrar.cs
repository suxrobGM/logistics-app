using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Tenant;

public static class Registrar
{
    public static IServiceCollection AddTenantApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(typeof(Registrar).Assembly);
        return services;
    }
}
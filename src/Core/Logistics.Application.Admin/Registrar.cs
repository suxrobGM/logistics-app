using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Admin;

public static class Registrar
{
    public static IServiceCollection AddAdminApplicationLayer(this IServiceCollection services)
    {
        services.AddMediatR(typeof(Registrar).Assembly);
        return services;
    }
}
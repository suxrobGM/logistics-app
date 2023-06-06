using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Tenant;

public static class Registrar
{
    public static IServiceCollection AddTenantApplicationLayer(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly));
        return services;
    }
}
using System.Reflection;
using FluentValidation;
using Logistics.Application.Tenant.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Tenant;

public static class Registrar
{
    public static IServiceCollection AddTenantApplicationLayer(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly));
        services.AddSingleton<IPushNotification, PushNotification>();
        return services;
    }
}

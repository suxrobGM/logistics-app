using System.Reflection;
using FluentValidation;
using Logistics.Application.Behaviours;
using Logistics.Application.Services;
using Logistics.Application.Services.Tax;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application;

public static class Registrar
{
    public static IServiceCollection AddApplicationTaxServices(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceTaxApplier, InvoiceTaxApplier>();
        return services;
    }

    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(FeatureCheckBehaviour<,>));
        });

        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<ILoadService, LoadService>();
        services.AddScoped<IMaintenanceReminderService, MaintenanceReminderService>();
        services.AddApplicationTaxServices();
        return services;
    }
}

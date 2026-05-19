using System.Reflection;
using FluentValidation;
using Logistics.Application.Behaviours;
using Logistics.Application.Modules.Compliance;
using Logistics.Application.Modules.Financial;
using Logistics.Application.Modules.IdentityAccess;
using Logistics.Application.Modules.Integrations;
using Logistics.Application.Modules.Operations;
using Logistics.Application.Modules.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application;

public static class Registrar
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddApplicationCommon();
        services.AddApplicationServices();

        services.AddOperationsModule();
        services.AddComplianceModule();
        services.AddFinancialModule();
        services.AddIdentityAccessModule();
        services.AddIntegrationsModule();
        services.AddPlatformModule();

        return services;
    }

    /// <summary>
    /// Registers tax-related application services (currently <see cref="Modules.Financial.Tax.Services.IInvoiceTaxApplier"/>).
    /// Used by hosts like DbMigrator that need to apply tax during seeding but don't pull in the
    /// full <see cref="AddApplicationLayer"/> stack (which also registers services that depend on
    /// SignalR, blob storage, and other infrastructure those hosts don't wire).
    /// </summary>
    public static IServiceCollection AddApplicationTaxServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(Registrar).Assembly)
            .AddClasses(c => c.InNamespaces("Logistics.Application.Modules.Financial.Tax.Services"), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(Registrar).Assembly)
            .AddClasses(c => c.AssignableTo<IApplicationService>(), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        return services;
    }

    private static IServiceCollection AddApplicationCommon(this IServiceCollection services)
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
        return services;
    }

}

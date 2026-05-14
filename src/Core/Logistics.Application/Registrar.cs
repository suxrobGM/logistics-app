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
    /// Kept for backward compatibility with callers in Program.cs files that invoke it
    /// independently. Scrutor now covers IInvoiceTaxApplier via the IApplicationService scan.
    /// </summary>
    public static IServiceCollection AddApplicationTaxServices(this IServiceCollection services)
        => services;

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

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(Registrar).Assembly)
            .AddClasses(c => c.AssignableTo<IApplicationService>(), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        return services;
    }
}

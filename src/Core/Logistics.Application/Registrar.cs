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

    /// <summary>
    /// Registers fuel-card application services (<see cref="Modules.Integrations.FuelCards.Services.IFuelCardSyncService"/>).
    /// Used by hosts like DbMigrator that seed fuel card demo data without pulling in the full
    /// <see cref="AddApplicationLayer"/> stack.
    /// </summary>
    public static IServiceCollection AddApplicationFuelCardServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(Registrar).Assembly)
            .AddClasses(c => c.InNamespaces("Logistics.Application.Modules.Integrations.FuelCards.Services"), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        return services;
    }

    /// <summary>
    /// Registers only the Compliance/Privacy MediatR slice, for hosts (the IdentityServer) that serve
    /// the GDPR self-service page without the full <see cref="AddApplicationLayer"/> stack.
    /// </summary>
    public static IServiceCollection AddApplicationPrivacyServices(this IServiceCollection services)
    {
        const string privacyNamespace = "Logistics.Application.Modules.Compliance.Privacy";

        // The filter isn't tidiness: an unfiltered scan registers notification handlers that
        // DispatchDomainEventsInterceptor would construct on SaveChanges. FeatureCheck is off - it
        // needs IFeatureService + ICurrentTenantAccessor, which this host doesn't wire.
        return services.AddApplicationCommon(
            typeFilter: type => type.Namespace?.StartsWith(privacyNamespace, StringComparison.Ordinal) == true,
            withFeatureCheck: false);
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // UserPermissionService caches on it; idempotent, so a host registering it too is fine.
        services.AddMemoryCache();

        services.Scan(scan => scan
            .FromAssemblies(typeof(Registrar).Assembly)
            .AddClasses(c => c.AssignableTo<IApplicationService>(), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        return services;
    }

    /// <summary>
    /// Validators + MediatR with the core pipeline. <paramref name="typeFilter"/> narrows the scan
    /// to one slice, for hosts that don't want the whole Application layer.
    /// </summary>
    private static IServiceCollection AddApplicationCommon(
        this IServiceCollection services,
        Func<Type, bool>? typeFilter = null,
        bool withFeatureCheck = true)
    {
        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly(),
            includeInternalTypes: true,
            filter: typeFilter is null ? null : result => typeFilter(result.ValidatorType));

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly);
            if (typeFilter is not null)
            {
                cfg.TypeEvaluator = typeFilter;
            }

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            if (withFeatureCheck)
            {
                cfg.AddOpenBehavior(typeof(FeatureCheckBehaviour<,>));
            }
        });
        return services;
    }

}

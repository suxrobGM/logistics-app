using System.Reflection;
using FluentValidation;
using Logistics.Application.Behaviours;
using Logistics.Mediator;
using Logistics.Application.Modules.Platform.ProductLicense.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplicationLayer()
        {
            services.AddApplicationCommon();
            services.AddApplicationServices();

            // Singleton: importing the public key once is enough, and the validator holds no state.
            services.AddSingleton(_ => new ProductLicenseKeyValidator(ProductLicensePublicKey.SpkiBase64));

            return services;
        }

        /// <summary>
        /// Registers only the tax services, for hosts like DbMigrator that can't call <see cref="AddApplicationLayer"/>
        /// because they have no SignalR or blob storage.
        /// </summary>
        public IServiceCollection AddApplicationTaxServices()
        {
            services.Scan(scan => scan
                .FromAssemblies(typeof(Registrar).Assembly)
                .AddClasses(c => c.InNamespaces("Logistics.Application.Modules.Financial.Tax.Services"), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            return services;
        }

        /// <summary>Registers only the fuel card services, for hosts like DbMigrator.</summary>
        public IServiceCollection AddApplicationFuelCardServices()
        {
            services.Scan(scan => scan
                .FromAssemblies(typeof(Registrar).Assembly)
                .AddClasses(c => c.InNamespaces("Logistics.Application.Modules.Integrations.FuelCards.Services"), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            return services;
        }

        /// <summary>Registers only the payroll services, for hosts like DbMigrator.</summary>
        public IServiceCollection AddApplicationPayrollServices()
        {
            services.Scan(scan => scan
                .FromAssemblies(typeof(Registrar).Assembly)
                .AddClasses(c => c.AssignableTo<Modules.Financial.Payroll.Services.IPayrollService>(), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            return services;
        }

        /// <summary>Registers only the GDPR export and account-deletion handlers, for the IdentityServer.</summary>
        public IServiceCollection AddApplicationPrivacyServices()
        {
            const string privacyNamespace = "Logistics.Application.Modules.Compliance.Privacy";

            // Without the filter, every domain event handler would be registered, and they need services the IdentityServer
            // doesn't have. The feature check needs them too.
            return services.AddApplicationCommon(
                typeFilter: type => type.Namespace?.StartsWith(privacyNamespace, StringComparison.Ordinal) == true,
                withFeatureCheck: false);
        }

        private IServiceCollection AddApplicationServices()
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

        /// <summary><paramref name="typeFilter"/> limits which handlers and validators are registered, for hosts that need only a few.</summary>
        private IServiceCollection AddApplicationCommon(
            Func<Type, bool>? typeFilter = null,
            bool withFeatureCheck = true)
        {
            services.AddValidatorsFromAssembly(
                Assembly.GetExecutingAssembly(),
                includeInternalTypes: true,
                filter: typeFilter is null ? null : result => typeFilter(result.ValidatorType));

            services.AddMediator(typeof(Registrar).Assembly, typeFilter);

            // Registration order is pipeline order: the first registered behaviour runs outermost.
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            if (withFeatureCheck)
            {
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FeatureCheckBehaviour<,>));
            }

            return services;
        }
    }

}

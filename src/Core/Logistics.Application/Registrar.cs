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

        /// <summary>For hosts like DbMigrator that skip <see cref="AddApplicationLayer"/>, whose services need SignalR and blob storage.</summary>
        public IServiceCollection AddApplicationTaxServices()
        {
            services.Scan(scan => scan
                .FromAssemblies(typeof(Registrar).Assembly)
                .AddClasses(c => c.InNamespaces("Logistics.Application.Modules.Financial.Tax.Services"), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            return services;
        }

        /// <summary>For hosts like DbMigrator that skip <see cref="AddApplicationLayer"/>.</summary>
        public IServiceCollection AddApplicationFuelCardServices()
        {
            services.Scan(scan => scan
                .FromAssemblies(typeof(Registrar).Assembly)
                .AddClasses(c => c.InNamespaces("Logistics.Application.Modules.Integrations.FuelCards.Services"), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            return services;
        }

        /// <summary>For hosts like DbMigrator that skip <see cref="AddApplicationLayer"/>.</summary>
        public IServiceCollection AddApplicationPayrollServices()
        {
            services.Scan(scan => scan
                .FromAssemblies(typeof(Registrar).Assembly)
                .AddClasses(c => c.AssignableTo<Modules.Financial.Payroll.Services.IPayrollService>(), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            return services;
        }

        /// <summary>The GDPR self-service slice, for the IdentityServer, which skips <see cref="AddApplicationLayer"/>.</summary>
        public IServiceCollection AddApplicationPrivacyServices()
        {
            const string privacyNamespace = "Logistics.Application.Modules.Compliance.Privacy";

            // Unfiltered, SaveChanges would build every notification handler; FeatureCheck needs services this host lacks.
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

        /// <summary><paramref name="typeFilter"/> narrows the scan to one slice for hosts that skip the rest.</summary>
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>Each provider is keyed by its provider-type enum value; factories resolve it with <see cref="GetRequiredProvider{TService}"/>.</summary>
public static class ProviderRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddProviderIntegration<TOptions, TFactoryService, TFactoryImplementation>(
            IConfiguration configuration, string sectionName)
            where TOptions : class
            where TFactoryService : class
            where TFactoryImplementation : class, TFactoryService
        {
            services.Configure<TOptions>(configuration.GetSection(sectionName));
            services.AddScoped<TFactoryService, TFactoryImplementation>();
            return services;
        }

        /// <summary>
        /// Maps <paramref name="providerType"/> to <typeparamref name="TImplementation"/>. Register the implementation
        /// itself as well (as a typed HttpClient or a scoped service); that registration decides its lifetime.
        /// </summary>
        public IServiceCollection AddKeyedProvider<TService, TImplementation>(Enum providerType)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddKeyedTransient<TService>(providerType, (sp, _) => sp.GetRequiredService<TImplementation>());
            return services;
        }
    }

    extension(IServiceProvider serviceProvider)
    {
        /// <exception cref="NotSupportedException">No provider is registered for that type.</exception>
        public TService GetRequiredProvider<TService>(Enum providerType, string familyName)
            where TService : notnull
        {
            if (!serviceProvider.HasProvider<TService>(providerType))
            {
                throw new NotSupportedException($"{familyName} provider '{providerType}' is not supported");
            }

            return serviceProvider.GetRequiredKeyedService<TService>(providerType);
        }

        /// <summary>Whether a <typeparamref name="TService"/> is registered for <paramref name="providerType"/>.</summary>
        public bool HasProvider<TService>(Enum providerType) =>
            serviceProvider.GetRequiredService<IServiceProviderIsKeyedService>()
                .IsKeyedService(typeof(TService), providerType);
    }
}

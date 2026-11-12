using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>
/// DI helpers shared by the third-party integration families (ELD, load board, fuel card, accounting).
/// Each provider is a keyed service under its provider-type enum value, so a family's factory
/// resolves it with <see cref="GetRequiredProvider{TService}"/>.
/// </summary>
public static class ProviderRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Binds <typeparamref name="TOptions"/> to <paramref name="sectionName"/> and registers the
        /// provider factory (<typeparamref name="TFactoryImplementation"/> as
        /// <typeparamref name="TFactoryService"/>) with a scoped lifetime.
        /// </summary>
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
        /// Registers <typeparamref name="TImplementation"/> as the <typeparamref name="TService"/> keyed by
        /// <paramref name="providerType"/>. The implementation keeps its own registration (typed
        /// <c>HttpClient</c> or scoped), and that lifetime is what the factory hands out.
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
        /// <summary>
        /// Resolves the <typeparamref name="TService"/> registered for <paramref name="providerType"/>.
        /// </summary>
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

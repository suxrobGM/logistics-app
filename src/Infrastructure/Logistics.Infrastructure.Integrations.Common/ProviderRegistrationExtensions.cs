using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>
/// DI helpers shared by the third-party integration registrars. Binds a provider family's options
/// section and registers its factory; the per-provider <c>AddHttpClient&lt;...&gt;</c> registrations
/// stay in each family's registrar since those provider services differ.
/// </summary>
public static class ProviderRegistrationExtensions
{
    /// <summary>
    /// Binds <typeparamref name="TOptions"/> to <paramref name="sectionName"/> and registers the
    /// provider factory (<typeparamref name="TFactoryImplementation"/> as
    /// <typeparamref name="TFactoryService"/>) with a scoped lifetime.
    /// </summary>
    public static IServiceCollection AddProviderIntegration<TOptions, TFactoryService, TFactoryImplementation>(
        this IServiceCollection services, IConfiguration configuration, string sectionName)
        where TOptions : class
        where TFactoryService : class
        where TFactoryImplementation : class, TFactoryService
    {
        services.Configure<TOptions>(configuration.GetSection(sectionName));
        services.AddScoped<TFactoryService, TFactoryImplementation>();
        return services;
    }
}

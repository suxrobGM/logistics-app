using Logistics.Application.Abstractions.Accounting;
using Logistics.Infrastructure.Integrations.Accounting.Providers;
using Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;
using Logistics.Infrastructure.Integrations.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations.Accounting;

public static class Registrar
{
    /// <summary>
    ///     Add accounting provider integrations (QuickBooks Online, Demo).
    /// </summary>
    public static IServiceCollection AddAccountingIntegrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<QuickBooksOAuthClient>();
        services.AddHttpClient<QuickBooksOnlineService>();
        services.AddScoped<DemoAccountingService>();

        services.AddProviderIntegration<AccountingOptions, IAccountingProviderFactory, AccountingProviderFactory>(
            configuration, AccountingOptions.SectionName);
        return services;
    }
}

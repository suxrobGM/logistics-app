using Logistics.Application.Abstractions.Accounting;
using Logistics.Infrastructure.Integrations.Accounting.Providers;
using Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;
using Logistics.Infrastructure.Integrations.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations.Accounting;

public static class Registrar
{
    extension(IServiceCollection services)
    {
            /// <summary>
            ///     Add accounting provider integrations (QuickBooks Online, Demo).
            /// </summary>
            public IServiceCollection AddAccountingIntegrations(
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
}

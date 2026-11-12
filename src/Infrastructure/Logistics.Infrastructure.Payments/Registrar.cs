using Logistics.Infrastructure.Payments.Stripe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.Payments;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Infrastructure.Payments;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Add payment infrastructure (Stripe standard + Connect).
        /// </summary>
        public IServiceCollection AddPaymentsInfrastructure(
            IConfiguration configuration)
        {
            // Configuration
            services.Configure<StripeOptions>(
                configuration.GetSection(StripeOptions.SectionName));

            // Bootstraps Stripe.NET's static StripeConfiguration.ApiKey once at host startup so
            // every Stripe-using service (here and in Logistics.Infrastructure.Tax) can rely on it
            // without repeating the init.
            services.AddHostedService<StripeApiKeyInitializer>();

            // Stripe services
            services.AddSingleton<IStripeWebhookParser, StripeWebhookParser>();
            services.AddSingleton<IStripeCustomerService, StripeCustomerService>();
            services.AddScoped<IStripeSubscriptionService, StripeSubscriptionService>();
            services.AddScoped<IStripePlanService, StripePlanService>();
            services.AddSingleton<IStripePaymentService, StripePaymentService>();
            services.AddSingleton<IStripeConnectService, StripeConnectService>();
            services.AddSingleton<IStripePortalService, StripePortalService>();
            services.AddScoped<IStripeUsageService, StripeUsageService>();

            return services;
        }
    }
}

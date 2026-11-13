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
        public IServiceCollection AddPaymentsInfrastructure(
            IConfiguration configuration)
        {
            services.Configure<StripeOptions>(
                configuration.GetSection(StripeOptions.SectionName));

            // Sets Stripe.NET's static API key once.
            services.AddHostedService<StripeApiKeyInitializer>();

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

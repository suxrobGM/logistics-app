using Logistics.Application.Services;
using Logistics.Infrastructure.Payments.Stripe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Payments;

public static class Registrar
{
    /// <summary>
    ///     Add payment infrastructure (Stripe standard + Connect).
    /// </summary>
    public static IServiceCollection AddPaymentsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<StripeOptions>(
            configuration.GetSection(StripeOptions.SectionName));

        // Stripe services
        services.AddSingleton<IStripeService, StripeService>();
        services.AddSingleton<IStripeCustomerService, StripeCustomerService>();
        services.AddSingleton<IStripeSubscriptionService, StripeSubscriptionService>();
        services.AddSingleton<IStripePaymentService, StripePaymentService>();
        services.AddSingleton<IStripeConnectService, StripeConnectService>();

        return services;
    }
}

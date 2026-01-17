using System.Reflection;
using FluentValidation;
using Logistics.Application.Behaviours;
using Logistics.Application.Hubs;
using Logistics.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application;

public static class Registrar
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string captchaConfigSection = "GoogleRecaptcha",
        string stripeSection = "Stripe")
    {
        var googleRecaptchaOptions = configuration.GetSection(captchaConfigSection).Get<GoogleRecaptchaOptions>();
        var stripeOptions = configuration.GetSection(stripeSection).Get<StripeOptions>();

        if (googleRecaptchaOptions is not null)
        {
            services.Configure<GoogleRecaptchaOptions>(options =>
            {
                options.SecretKey = googleRecaptchaOptions.SecretKey;
                options.SiteKey = googleRecaptchaOptions.SiteKey;
            });
            services.AddSingleton<ICaptchaService, GoogleRecaptchaService>();
        }

        if (stripeOptions is not null)
        {
            services.Configure<StripeOptions>(options =>
            {
                options.PublishableKey = stripeOptions.PublishableKey;
                options.SecretKey = stripeOptions.SecretKey;
                options.WebhookSecret = stripeOptions.WebhookSecret;
            });
            services.AddSingleton<IStripeService, StripeService>();
        }

        services.AddSignalR();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        services.AddSingleton<IPushNotificationService, PushNotificationService>();
        services.AddSingleton<IStripeService, StripeService>();
        services.AddSingleton<LiveTrackingHubContext>();
        services.AddSingleton<MessagingHubContext>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ILoadService, LoadService>();
        return services;
    }
}

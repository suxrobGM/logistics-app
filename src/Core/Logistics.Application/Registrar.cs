using System.Reflection;
using FluentValidation;
using Logistics.Application.Behaviours;
using Logistics.Application.Hubs;
using Logistics.Application.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application;

public static class Registrar
{
    public static IServiceCollection AddApplicationLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string emailConfigSection = "SmtpConfig",
        string captchaConfigSection = "GoogleRecaptchaConfig",
        string stripeConfigSection = "StripeConfig")
    {
        var emailSenderOptions = configuration.GetSection(emailConfigSection).Get<SmtpOptions>();
        var googleRecaptchaOptions = configuration.GetSection(captchaConfigSection).Get<GoogleRecaptchaOptions>();
        var stripeOptions = configuration.GetSection(stripeConfigSection).Get<StripeOptions>();

        if (emailSenderOptions is not null)
        {
            services.Configure<SmtpOptions>(options =>
            {
                options.Host = emailSenderOptions.Host;
                options.Port = emailSenderOptions.Port;
                options.UserName = emailSenderOptions.UserName;
                options.Password = emailSenderOptions.Password;
                options.SenderName = emailSenderOptions.SenderName;
                options.SenderEmail = emailSenderOptions.SenderEmail;
            });
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        }

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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly));
        
        services.AddSingleton<IPushNotificationService, PushNotificationService>();
        services.AddSingleton<IStripeService, StripeService>();
        services.AddSingleton<LiveTrackingHubContext>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<INotificationService, NotificationService>();
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        return services;
    }
}

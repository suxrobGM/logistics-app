using System.Reflection;
using FluentValidation;
using Logistics.Application.Behaviours;
using Logistics.Application.Services;
using Logistics.Application.Services.Implementations;
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
        string captchaConfigSection = "GoogleRecaptchaConfig")
    {
        var emailSenderOptions = configuration.GetSection(emailConfigSection).Get<SmtpOptions>();
        var googleRecaptchaOptions = configuration.GetSection(captchaConfigSection).Get<GoogleRecaptchaOptions>();

        if (emailSenderOptions is not null)
        {
            services.AddSingleton(emailSenderOptions);
            services.AddTransient<IEmailSender, SmtpEmailSender>();
        }

        if (googleRecaptchaOptions is not null)
        {
            services.AddSingleton(googleRecaptchaOptions);
            services.AddScoped<ICaptchaService, GoogleRecaptchaService>();
        }
        
        services.AddSingleton<IPushNotificationService, PushNotificationService>();
        services.AddScoped<IPayrollService, PayrollService>();
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly));
        return services;
    }
}

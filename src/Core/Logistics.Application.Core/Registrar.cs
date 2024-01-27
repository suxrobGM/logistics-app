using Logistics.Application.Core.Behaviours;
using Logistics.Application.Core.Services;
using Logistics.Application.Core.Services.Implementations;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Core;

public static class Registrar
{
    public static IServiceCollection AddApplicationCoreLayer(
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
        
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        return services;
    }
}

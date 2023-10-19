using Logistics.Application.Core.Behaviours;
using Logistics.Application.Core.Options;
using Logistics.Application.Core.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Core;

public static class Registrar
{
    public static IServiceCollection AddApplicationCoreLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string emailConfigSection = "Email",
        string captchaConfigSection = "Captcha")
    {

        var emailSenderOptions = configuration.GetSection(emailConfigSection).Get<EmailSenderOptions>();
        var googleRecaptchaOptions = configuration.GetSection(captchaConfigSection).Get<GoogleRecaptchaOptions>();

        if (emailSenderOptions != null)
        {
            services.AddSingleton(emailSenderOptions);
            services.AddTransient<IEmailSender, EmailSender>();
        }

        if (googleRecaptchaOptions != null)
        {
            services.AddSingleton(googleRecaptchaOptions);
            services.AddScoped<ICaptchaService, GoogleRecaptchaService>();
        }
        
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        return services;
    }
}

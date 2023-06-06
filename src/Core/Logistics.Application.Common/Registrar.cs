using Logistics.Application.Common.Options;
using Logistics.Application.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Common;

public static class Registrar
{
    public static IServiceCollection AddSharedApplicationLayer(
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
        return services;
    }
}
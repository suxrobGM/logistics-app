using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Shared.Options;
using Logistics.Application.Shared.Services;

namespace Logistics.Application.Shared;

public static class ServiceCollectionExtensions
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
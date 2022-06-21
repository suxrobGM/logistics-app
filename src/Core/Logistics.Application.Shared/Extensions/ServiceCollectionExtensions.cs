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

        services.AddSingleton(emailSenderOptions);
        services.AddSingleton(googleRecaptchaOptions);
        services.AddTransient<IEmailSender, EmailSender>();
        services.AddScoped<ICaptchaService, GoogleRecaptchaService>();
        return services;
    }
}
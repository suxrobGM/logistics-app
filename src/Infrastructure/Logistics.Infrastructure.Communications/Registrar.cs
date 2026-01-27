using Logistics.Application.Contracts.Services.Email;
using Logistics.Application.Services;
using Logistics.Application.Services.Realtime;
using Logistics.Infrastructure.Communications.Captcha;
using Logistics.Infrastructure.Communications.Email;
using Logistics.Infrastructure.Communications.Services;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Infrastructure.Communications.SignalR.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Communications;

public static class Registrar
{
    /// <summary>
    ///     Add communications infrastructure (SignalR, Email, Push Notifications).
    /// </summary>
    public static IServiceCollection AddCommunicationsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // SignalR real-time communication
        services.AddSignalR();
        services.AddSingleton<LiveTrackingHubContext>();
        services.AddSingleton<MessagingHubContext>();

        // SignalR abstractions for Application layer
        services.AddScoped<IRealtimeMessagingService, SignalRMessagingService>();
        services.AddScoped<IRealtimeLiveTrackingService, SignalRLiveTrackingService>();
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();
        services.AddScoped<ITripTrackingService, SignalRTripTrackingService>();

        // Email services
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IEmailTemplateService, FluidEmailTemplateService>();

        // Push notifications
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<IPushNotificationService, PushNotificationService>();

        // Google Recaptcha
        services.Configure<GoogleRecaptchaOptions>(configuration.GetSection(GoogleRecaptchaOptions.SectionName));
        services.AddSingleton<ICaptchaService, GoogleRecaptchaService>();

        return services;
    }
}

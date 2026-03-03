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
using Resend;

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
        services.AddSingleton<TrackingHubContext>();
        services.AddSingleton<ChatHubContext>();

        // SignalR abstractions for Application layer
        services.AddScoped<IRealtimeMessagingService, SignalRMessagingService>();
        services.AddScoped<IRealtimeLiveTrackingService, SignalRLiveTrackingService>();
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();
        services.AddScoped<ITripTrackingService, SignalRTripTrackingService>();

        // Email services (Resend)
        services.Configure<ResendOptions>(configuration.GetSection(ResendOptions.SectionName));
        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = configuration.GetSection(ResendOptions.SectionName)["ApiKey"]
                         ?? throw new InvalidOperationException("Resend:ApiKey is not configured");
        });
        services.AddTransient<IResend, ResendClient>();
        services.AddSingleton<IEmailSender, ResendEmailSender>();
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

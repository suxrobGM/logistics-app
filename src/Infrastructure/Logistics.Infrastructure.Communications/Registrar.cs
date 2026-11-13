using System.Text.Json;
using System.Text.Json.Serialization;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.Communications;
using Logistics.Application.Abstractions.Routing;
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
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCommunicationsInfrastructure(
            IConfiguration configuration)
        {
            // Send enums as snake_case strings, as the REST API does; the frontend compares them to strings, so numbers never match.
            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
            });
            services.AddSingleton<TrackingHubContext>();

            services.AddScoped<IRealtimeMessagingService, SignalRMessagingService>();
            services.AddScoped<IRealtimeLiveTrackingService, SignalRLiveTrackingService>();
            services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();
            services.AddScoped<ITripTrackingService, SignalRTripTrackingService>();
            services.AddScoped<IAIDispatchBroadcastService, SignalRAIDispatchBroadcastService>();
            services.AddScoped<IAICopilotBroadcastService, SignalRAICopilotBroadcastService>();

            services.Configure<ResendOptions>(configuration.GetSection(ResendOptions.SectionName));
            services.AddOptions();
            services.AddHttpClient<ResendClient>();
            services.Configure<ResendClientOptions>(o =>
            {
                o.ApiToken = configuration.GetSection(ResendOptions.SectionName)["ApiKey"]
                             ?? throw new InvalidOperationException("Resend:ApiKey is not configured");
            });
            services.AddTransient<IResend, ResendClient>();
            services.AddScoped<IEmailSender, ResendEmailSender>();
            services.AddScoped<IThreadedEmailSender, ResendThreadedEmailSender>();
            services.AddScoped<IInboundEmailReader, ResendInboundEmailReader>();
            services.AddScoped<IInboundEmailWebhookVerifier, ResendWebhookVerifier>();
            services.AddSingleton<IEmailTemplateService, FluidEmailTemplateService>();

            services.AddSingleton<IPushNotificationService, PushNotificationService>();

            services.Configure<GoogleRecaptchaOptions>(configuration.GetSection(GoogleRecaptchaOptions.SectionName));
            services.AddSingleton<ICaptchaService, GoogleRecaptchaService>();

            return services;
        }
    }
}

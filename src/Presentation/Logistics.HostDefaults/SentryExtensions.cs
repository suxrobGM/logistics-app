using Logistics.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentry.AspNetCore;
using Sentry.Extensibility;
using Serilog;
using Serilog.Events;

namespace Logistics.HostDefaults;

/// <summary>
/// Error tracking for the presentation hosts. Off unless <c>Sentry:Dsn</c> is configured; when on,
/// Serilog <c>Error</c>+ entries become Sentry events and lower levels ride along as breadcrumbs.
/// </summary>
public static class SentryExtensions
{
    private const string DsnKey = "Sentry:Dsn";

    public static bool IsSentryEnabled(this IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration[DsnKey]);

    /// <summary>
    /// Initialises the SDK from the <c>Sentry</c> configuration section. Environment falls back to
    /// <c>SENTRY_ENVIRONMENT</c>, then the host environment; release to <c>SENTRY_RELEASE</c>, then
    /// the product version. The portals read the same two variables, so all events line up.
    /// </summary>
    public static WebApplicationBuilder AddLogisticsSentry(this WebApplicationBuilder builder)
    {
        if (!builder.Configuration.IsSentryEnabled())
        {
            return builder;
        }

        builder.WebHost.UseSentry(options =>
        {
            options.Environment ??= Environment.GetEnvironmentVariable("SENTRY_ENVIRONMENT")
                                    ?? builder.Environment.EnvironmentName.ToLowerInvariant();
            options.Release ??= Environment.GetEnvironmentVariable("SENTRY_RELEASE") ?? ProductInfo.Version;
            options.DefaultTags["app"] = builder.Environment.ApplicationName;
            options.SendDefaultPii = false;
            options.MaxRequestBodySize = RequestSize.None;
            options.SetBeforeSend(SentryEventScrubber.Scrub);
            options.SetBeforeBreadcrumb(SentryEventScrubber.Scrub);
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ISentryEventProcessor, TenantSentryEventProcessor>();
        return builder;
    }

    /// <summary>
    /// Routes Serilog into the SDK that <see cref="AddLogisticsSentry"/> initialised.
    /// </summary>
    public static LoggerConfiguration WriteToSentryIfEnabled(
        this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        if (!configuration.IsSentryEnabled())
        {
            return loggerConfiguration;
        }

        return loggerConfiguration.WriteTo.Sentry(options =>
        {
            options.InitializeSdk = false;
            options.MinimumEventLevel = LogEventLevel.Error;
            options.MinimumBreadcrumbLevel = LogEventLevel.Information;
        });
    }
}

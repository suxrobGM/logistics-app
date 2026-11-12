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

/// <summary>Off unless <c>Sentry:Dsn</c> is set; then Serilog errors become events and lower levels breadcrumbs.</summary>
public static class SentryExtensions
{
    private const string DsnKey = "Sentry:Dsn";

    extension(IConfiguration configuration)
    {
        public bool IsSentryEnabled() =>
            !string.IsNullOrWhiteSpace(configuration[DsnKey]);
    }

    extension(WebApplicationBuilder builder)
    {
        /// <summary>Reads <c>SENTRY_ENVIRONMENT</c> and <c>SENTRY_RELEASE</c> like the portals do, so their events line up.</summary>
        public WebApplicationBuilder AddLogisticsSentry()
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
    }

    extension(LoggerConfiguration loggerConfiguration)
    {
        /// <summary>Relies on the SDK that <see cref="AddLogisticsSentry"/> initialised.</summary>
        public LoggerConfiguration WriteToSentryIfEnabled(
            IConfiguration configuration)
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
}

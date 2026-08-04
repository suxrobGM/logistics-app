using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Logistics.HostDefaults;

/// <summary>
/// Shared host bootstrap for the presentation web apps. Owns the Serilog bootstrap logger,
/// the standard resilience handler, health-check wiring, and the top-level try/catch/finally
/// shell so each host's <c>Program.cs</c> collapses to a single <see cref="Run"/> call.
/// </summary>
public static class LogisticsHost
{
    public static void Run(string[] args, Func<WebApplicationBuilder, WebApplication> buildApp)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting up");

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHealthChecks();
            builder.Services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());

            builder.Host.UseSerilog((ctx, lc) =>
            {
                // ReadFrom.Configuration stays last so Serilog__MinimumLevel__* overrides these
                // defaults. It is additive for sinks, though, so a configured Serilog:WriteTo
                // replaces the defaults here rather than stacking a second copy on top.
                lc.MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore.DataProtection", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .Enrich.FromLogContext();

                if (!ctx.Configuration.GetSection("Serilog:WriteTo").Exists())
                {
                    lc.WriteTo.Console()
                        .WriteTo.File(
                            new CompactJsonFormatter(),
                            $"Logs/{ctx.HostingEnvironment.ApplicationName}-.log",
                            rollingInterval: RollingInterval.Month);
                }

                lc.ReadFrom.Configuration(ctx.Configuration);
            });

            var app = buildApp(builder);
            app.MapHealthChecks("/health");

            app.Run();
        }
        catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException") // https://github.com/dotnet/runtime/issues/60600
        {
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            Log.Information("Shut down complete");
            Log.CloseAndFlush();
        }
    }
}

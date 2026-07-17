using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

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

            builder.Host.UseSerilog((ctx, lc) => lc
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(ctx.Configuration));

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

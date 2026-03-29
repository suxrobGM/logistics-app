using Logistics.Application.Services;
using Logistics.TelegramBot.Authentication;
using Logistics.TelegramBot.Commands;
using Logistics.TelegramBot.Handlers;
using Logistics.TelegramBot.Options;
using Logistics.TelegramBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Logistics.TelegramBot;

public static class Registrar
{
    /// <summary>
    ///   Add Telegram Bot services and handlers
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="isDevelopment">If true, uses long polling instead of webhook (for local development)</param>
    public static IServiceCollection AddTelegramBotInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration, bool isDevelopment = false)
    {
        var section = configuration.GetSection(TelegramBotOptions.SectionName);
        services.Configure<TelegramBotOptions>(section);

        var botOptions = section.Get<TelegramBotOptions>();
        if (string.IsNullOrEmpty(botOptions?.BotToken))
            return services;

        // Telegram bot client (singleton — thread-safe)
        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botOptions.BotToken));

        // Authentication
        services.AddScoped<TelegramAuthService>();

        // Handlers
        services.AddScoped<TelegramUpdateDispatcher>();
        services.AddScoped<CommandRouter>();
        services.AddScoped<CallbackQueryHandler>();

        // Commands
        services.AddScoped<ITelegramCommand, StartCommand>();
        services.AddScoped<ITelegramCommand, DisconnectCommand>();
        services.AddScoped<ITelegramCommand, LoadsCommand>();
        services.AddScoped<ITelegramCommand, TrucksCommand>();
        services.AddScoped<ITelegramCommand, TripsCommand>();
        services.AddScoped<ITelegramCommand, HosCommand>();
        services.AddScoped<ITelegramCommand, NotifyCommand>();
        services.AddScoped<ITelegramCommand, HelpCommand>();

        // Services
        services.AddSingleton<ITelegramNotificationService, TelegramNotificationService>();

        // Background services
        services.AddHostedService<TelegramChatCacheWarmer>();

        // Development mode: use long polling
        if (isDevelopment)
        {
            services.AddHostedService<TelegramPollingService>();
        }

        return services;
    }

    public static WebApplication MapTelegramWebhook(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<TelegramBotOptions>>().Value;

        if (string.IsNullOrEmpty(options.BotToken))
            return app;

        // Production mode: register webhook with Telegram
        if (app.Environment.IsProduction() && !string.IsNullOrEmpty(options.WebhookUrl))
        {
            var capturedOptions = options;
            app.MapPost("/webhooks/telegram",
                (HttpContext context, IServiceScopeFactory scopeFactory) =>
                    TelegramWebhookHandler.HandleAsync(context, scopeFactory, capturedOptions))
                .AllowAnonymous();

            // Set webhook on startup
            var bot = app.Services.GetRequiredService<ITelegramBotClient>();
            _ = bot.SetWebhook(
                options.WebhookUrl,
                secretToken: options.SecretToken);

            // Delete webhook on shutdown
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                bot.DeleteWebhook().GetAwaiter().GetResult();
            });
        }

        return app;
    }
}

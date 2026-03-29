using Logistics.TelegramBot.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Logistics.TelegramBot.Services;

internal sealed class TelegramPollingService(
    ITelegramBotClient bot,
    IServiceScopeFactory scopeFactory,
    ILogger<TelegramPollingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Telegram bot polling service started");
        var offset = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await bot.GetUpdates(
                    offset: offset,
                    timeout: 30,
                    cancellationToken: stoppingToken);

                foreach (var update in updates)
                {
                    offset = update.Id + 1;
                    _ = ProcessUpdateAsync(update, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error polling Telegram updates");
                await Task.Delay(3000, stoppingToken);
            }
        }

        logger.LogInformation("Telegram bot polling service stopped");
    }

    private async Task ProcessUpdateAsync(Update update, CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<TelegramUpdateDispatcher>();
            await dispatcher.DispatchAsync(update, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Telegram update {UpdateId}", update.Id);
        }
    }
}

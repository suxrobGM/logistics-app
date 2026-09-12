using Logistics.Infrastructure.Integrations.Common;
using Logistics.TelegramBot.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Logistics.TelegramBot.Handlers;

internal static class TelegramWebhookHandler
{
    /// <summary>
    /// Handles incoming Telegram webhook updates. Rejects the request unless the secret token is
    /// configured and matches, then dispatches the update in a background scope.
    /// </summary>
    public static async Task HandleAsync(
        HttpContext context,
        IServiceScopeFactory scopeFactory,
        TelegramBotOptions options)
    {
        // The endpoint is anonymous, so this header is the only proof the caller is Telegram.
        var secretHeader = context.Request.Headers["X-Telegram-Bot-Api-Secret-Token"].FirstOrDefault();
        if (!WebhookSignature.ConstantTimeEquals(secretHeader, options.SecretToken))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var update = await context.Request.ReadFromJsonAsync<Update>(context.RequestAborted);
        if (update is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // Process in background scope to not block Telegram
        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<TelegramUpdateDispatcher>();
            await dispatcher.DispatchAsync(update, CancellationToken.None);
        });

        context.Response.StatusCode = StatusCodes.Status200OK;
    }
}

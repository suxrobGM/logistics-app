using System.Security.Cryptography;
using System.Text;
using Logistics.TelegramBot.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Logistics.TelegramBot.Handlers;

internal static class TelegramWebhookHandler
{
    /// <summary>
    /// Handles incoming Telegram webhook updates.
    /// Validates the secret token if configured, then processes the update in a background scope to avoid blocking Telegram's request.
    /// The update is dispatched to the TelegramUpdateDispatcher for handling.
    /// </summary>
    public static async Task HandleAsync(
        HttpContext context,
        IServiceScopeFactory scopeFactory,
        TelegramBotOptions options)
    {
        // Validate secret token
        if (!string.IsNullOrEmpty(options.SecretToken))
        {
            var secretHeader = context.Request.Headers["X-Telegram-Bot-Api-Secret-Token"].FirstOrDefault();
            if (!ConstantTimeEquals(secretHeader, options.SecretToken))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
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

    private static bool ConstantTimeEquals(string? a, string? b)
    {
        if (a is null || b is null)
            return false;

        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}

using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Notifications;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Platform.Notifications.Services;

internal sealed class EventNotifier(
    IPushNotificationService pushNotificationService,
    INotificationService notificationService,
    ITelegramNotificationService telegramNotificationService,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ITenantUnitOfWork tenantUow,
    ILogger<EventNotifier> logger) : IEventNotifier
{
    public async Task PushAsync(
        string title, string body, IReadOnlyDictionary<string, string>? data, params string?[] deviceTokens)
    {
        foreach (var token in deviceTokens.Where(t => !string.IsNullOrEmpty(t)).Distinct())
        {
            try
            {
                await pushNotificationService.SendNotificationAsync(title, body, token!, data);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send push notification '{Title}'", title);
            }
        }
    }

    public async Task ShowInAppAsync(string title, string message)
    {
        try
        {
            await notificationService.SendNotificationAsync(title, message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to save in-app notification '{Title}'", title);
        }
    }

    public async Task PostToTelegramAsync(string title, string message, TelegramChatRole? role, CancellationToken ct)
    {
        try
        {
            await telegramNotificationService.SendNotificationAsync(
                tenantUow.GetCurrentTenant().Id, title, message, role, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send Telegram notification '{Title}'", title);
        }
    }

    public async Task EmailAsync<TModel>(string? to, string subject, string template, TModel model)
        where TModel : class
    {
        if (string.IsNullOrEmpty(to))
        {
            return;
        }

        try
        {
            var htmlBody = await emailTemplateService.RenderAsync(template, model);
            await emailSender.SendEmailAsync(to, subject, htmlBody);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send '{Template}' email to {Email}", template, to);
        }
    }
}

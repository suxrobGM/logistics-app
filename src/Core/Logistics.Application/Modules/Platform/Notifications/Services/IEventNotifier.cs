using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Platform.Notifications.Services;

/// <summary>Every method logs and swallows its failures, since the change that raised the event is already saved.</summary>
public interface IEventNotifier : IApplicationService
{
    /// <summary>Pushes to every device token that is set; null and empty tokens are skipped.</summary>
    Task PushAsync(string title, string body, IReadOnlyDictionary<string, string>? data, params string?[] deviceTokens);

    /// <summary>Shows an in-app notification to the tenant's portal users.</summary>
    Task ShowInAppAsync(string title, string message);

    /// <summary>Posts to the tenant's Telegram chats for <paramref name="role" />, or every chat when null.</summary>
    Task PostToTelegramAsync(string title, string message, TelegramChatRole? role, CancellationToken ct);

    /// <summary>Renders <paramref name="template" /> and emails it; does nothing when <paramref name="to" /> is empty.</summary>
    Task EmailAsync<TModel>(string? to, string subject, string template, TModel model) where TModel : class;
}

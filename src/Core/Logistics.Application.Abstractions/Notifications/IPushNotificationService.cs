using Logistics.Application.Abstractions.Notifications;
namespace Logistics.Application.Abstractions.Notifications;

public interface IPushNotificationService
{
    Task SendNotificationAsync(string title, string body, string deviceToken, IReadOnlyDictionary<string, string>? data = null);
}

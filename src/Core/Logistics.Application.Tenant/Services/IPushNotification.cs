namespace Logistics.Application.Tenant.Services;

public interface IPushNotification
{
    Task SendNotificationAsync(string title, string body, string deviceToken, IReadOnlyDictionary<string, string>? data = null);
}

namespace Logistics.Application.Tenant.Services;

public interface IPushNotificationService
{
    Task SendNotificationAsync(string title, string body, string deviceToken, IReadOnlyDictionary<string, string>? data = null);
}

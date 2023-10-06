namespace Logistics.Application.Tenant.Services;

public interface INotificationService
{
    Task SendNotificationAsync(string title, string message);
}

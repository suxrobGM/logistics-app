namespace Logistics.Application.Services;

public interface INotificationService
{
    Task SendNotificationAsync(string title, string message);
}

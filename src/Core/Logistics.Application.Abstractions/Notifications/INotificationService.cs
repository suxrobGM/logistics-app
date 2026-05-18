using Logistics.Application.Abstractions.Notifications;
namespace Logistics.Application.Abstractions.Notifications;

public interface INotificationService
{
    Task SendNotificationAsync(string title, string message);
}

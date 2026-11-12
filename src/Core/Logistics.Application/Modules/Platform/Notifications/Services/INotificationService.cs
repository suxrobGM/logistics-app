namespace Logistics.Application.Modules.Platform.Notifications.Services;

/// <summary>Saves an in-app notification for the current tenant and broadcasts it to the portal.</summary>
public interface INotificationService : IApplicationService
{
    Task SendNotificationAsync(string title, string message);
}

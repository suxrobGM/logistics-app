using Logistics.Application.Tenant.Services;

namespace Logistics.Application.Tenant.Extensions;

internal static class PushNotificationExtensions
{
    public static async Task SendNewLoadNotificationAsync(this IPushNotification pushNotification, Load load, Truck truck)
    {
        var drivers = truck.Drivers.Where(driver => !string.IsNullOrEmpty(driver.DeviceToken));
        
        foreach (var driver in drivers)
        {
            await pushNotification.SendNotificationAsync(
                "Received a load",
                $"A new load #{load.RefId} has been assigned to you", 
                driver.DeviceToken!,
                new Dictionary<string, string> {{"loadId", load.Id}});
        }
    }
    
    public static async Task SendUpdatedLoadNotificationAsync(this IPushNotification pushNotification, Load load, Truck truck)
    {
        var drivers = truck.Drivers.Where(driver => !string.IsNullOrEmpty(driver.DeviceToken));
        
        foreach (var driver in drivers)
        {
            await pushNotification.SendNotificationAsync(
                "Load update",
                $"A load #{load.RefId} details has been updated check details", 
                driver.DeviceToken!,
                new Dictionary<string, string> {{"loadId", load.Id}});
        }
    }
    
    public static async Task SendRemovedLoadNotificationAsync(this IPushNotification pushNotification, Load load, Truck truck)
    {
        var drivers = truck.Drivers.Where(driver => !string.IsNullOrEmpty(driver.DeviceToken));
        
        foreach (var driver in drivers)
        {
            await pushNotification.SendNotificationAsync(
                "Load update",
                $"A load #{load.RefId} has been removed from you", 
                driver.DeviceToken!,
                new Dictionary<string, string> {{"loadId", load.Id}});
        }
    }
}

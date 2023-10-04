using Logistics.Application.Tenant.Services;
using Logistics.Domain.Enums;

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
    
    public static async Task SendConfirmLoadStatusNotificationAsync(this IPushNotification pushNotification, Load load, LoadStatus loadStatus)
    {
        if (load.AssignedTruck is null)
        {
            return;
        }
        
        var drivers = load.AssignedTruck.Drivers.Where(driver => !string.IsNullOrEmpty(driver.DeviceToken));

        var loadStatusText = loadStatus switch
        {
            LoadStatus.PickedUp => "pick up",
            LoadStatus.Delivered => "delivered",
        };

        foreach (var driver in drivers)
        {
            await pushNotification.SendNotificationAsync(
                "Confirm load status",
                $"You can confirm the {loadStatusText} date of load #{load.RefId}", 
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

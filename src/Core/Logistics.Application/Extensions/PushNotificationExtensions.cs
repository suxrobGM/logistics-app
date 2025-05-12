using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Shared.Consts;

namespace Logistics.Application.Extensions;

internal static class PushNotificationExtensions
{
    public static async Task SendNewLoadNotificationAsync(this IPushNotificationService pushNotificationService, Load load, Truck truck)
    {
        var drivers = truck.Drivers.Where(driver => !string.IsNullOrEmpty(driver.DeviceToken));
        
        foreach (var driver in drivers)
        {
            await pushNotificationService.SendNotificationAsync(
                "Received a load",
                $"A new load #{load.Number} has been assigned to you", 
                driver.DeviceToken!,
                new Dictionary<string, string> {{"loadId", load.Id.ToString()}});
        }
    }
    
    public static async Task SendConfirmLoadStatusNotificationAsync(this IPushNotificationService pushNotificationService, Load load, LoadStatus loadStatus)
    {
        if (load.AssignedTruck is null)
        {
            return;
        }
        
        var drivers = load.AssignedTruck.Drivers.Where(driver => !string.IsNullOrEmpty(driver.DeviceToken));

        var loadStatusText = loadStatus switch
        {
            LoadStatus.PickedUp => "picked up",
            LoadStatus.Delivered => "delivered",
            LoadStatus.Dispatched => "dispatched",
            _ => throw new ArgumentOutOfRangeException(nameof(loadStatus), loadStatus, null)
        };

        foreach (var driver in drivers)
        {
            await pushNotificationService.SendNotificationAsync(
                "Confirm load status",
                $"You can confirm the {loadStatusText} date of load #{load.Number}", 
                driver.DeviceToken!,
                new Dictionary<string, string> {{"loadId", load.Id.ToString()}});
        }
    }
    
    public static async Task SendUpdatedLoadNotificationAsync(this IPushNotificationService pushNotificationService, Load load, Truck truck)
    {
        var drivers = truck.Drivers.Where(driver => !string.IsNullOrEmpty(driver.DeviceToken));
        
        foreach (var driver in drivers)
        {
            await pushNotificationService.SendNotificationAsync(
                "Load update",
                $"A load #{load.Number} details has been updated check details", 
                driver.DeviceToken!,
                new Dictionary<string, string> {{"loadId", load.Id.ToString()}});
        }
    }
    
    public static async Task SendRemovedLoadNotificationAsync(this IPushNotificationService pushNotificationService, Load load, Truck? truck)
    {
        if (truck is null)
        {
            return;
        }
        
        var drivers = truck.Drivers.Where(driver => !string.IsNullOrEmpty(driver.DeviceToken));
        
        foreach (var driver in drivers)
        {
            await pushNotificationService.SendNotificationAsync(
                "Load update",
                $"A load #{load.Number} has been removed from you", 
                driver.DeviceToken!,
                new Dictionary<string, string> {{"loadId", load.Id.ToString()}});
        }
    }
}

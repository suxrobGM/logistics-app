using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Shared.Consts;

namespace Logistics.Application.Extensions;

internal static class PushNotificationExtensions
{
    public static Task SendNewLoadNotificationAsync(this IPushNotificationService pushNotificationService, Load load, Truck truck)
    {
        return pushNotificationService.SendToDriversAsync(
            truck.GetActiveDrivers(),
            "Received a load",
            $"A new load #{load.Number} has been assigned to you",
            load);
    }
    
    public static Task SendConfirmLoadStatusNotificationAsync(this IPushNotificationService pushNotificationService, Load load, LoadStatus loadStatus)
    {
        var truck = load.AssignedTruck;
        if (truck is null) return Task.CompletedTask;

        var statusText = loadStatus switch
        {
            LoadStatus.PickedUp   => "picked up",
            LoadStatus.Delivered  => "delivered",
            LoadStatus.Dispatched => "dispatched",
            _ => throw new ArgumentOutOfRangeException(nameof(loadStatus), loadStatus, null)
        };

        return pushNotificationService.SendToDriversAsync(
            truck.GetActiveDrivers(),
            "Confirm load status",
            $"You can confirm the {statusText} date of load #{load.Number}",
            load);
    }
    
    public static Task SendUpdatedLoadNotificationAsync(this IPushNotificationService pushNotificationService, Load load, Truck truck)
    {
        return pushNotificationService.SendToDriversAsync(
            truck.GetActiveDrivers(),
            "Load update",
            $"Load #{load.Number} details have been updated. Check details.",
            load);
    }
    
    public static Task SendRemovedLoadNotificationAsync(this IPushNotificationService pushNotificationService, Load load, Truck? truck)
    {
        if (truck is null)
        {
            return Task.CompletedTask;
        }

        return pushNotificationService.SendToDriversAsync(
            truck.GetActiveDrivers(),
            "Load update",
            $"Load #{load.Number} has been removed from you",
            load);
    }

    #region Helpers

    /// <summary>
    /// Returns every driver (main and secondary) that has a usable FCM device token.
    /// </summary>
    private static IEnumerable<Employee> GetActiveDrivers(this Truck truck)
    {
        if (truck.MainDriver is { DeviceToken: { Length: > 0 } })
            yield return truck.MainDriver;

        if (truck.SecondaryDriver is { DeviceToken: { Length: > 0 } } driver)
            yield return driver;
    }
    
    /// <summary>
    /// Sends the same push payload to each supplied driver.
    /// </summary>
    private static async Task SendToDriversAsync(
        this IPushNotificationService svc,
        IEnumerable<Employee> drivers,
        string title,
        string body,
        Load load)
    {
        var data = new Dictionary<string, string> { ["loadId"] = load.Id.ToString() };

        foreach (var driver in drivers)
        {
            await svc.SendNotificationAsync(title, body, driver.DeviceToken!, data);
        }
    }

    #endregion
}

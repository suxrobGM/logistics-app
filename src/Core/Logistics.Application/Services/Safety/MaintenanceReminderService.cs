using Logistics.Application.Services.Realtime;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Maintenance;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Services;

internal class MaintenanceReminderService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IRealtimeNotificationService notificationService,
    ILogger<MaintenanceReminderService> logger)
    : IMaintenanceReminderService
{
    private static readonly int[] ReminderDays = [30, 14, 7, 3, 1];

    public async Task ProcessMaintenanceRemindersAsync(CancellationToken ct = default)
    {
        var tenants = await masterUow.Repository<Tenant>().GetListAsync();

        foreach (var tenant in tenants)
        {
            try
            {
                tenantUow.SetCurrentTenant(tenant);
                await ProcessTenantMaintenanceAsync(tenant, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing maintenance for tenant {TenantName}", tenant.Name);
            }
        }
    }

    private async Task ProcessTenantMaintenanceAsync(Tenant tenant, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var maxDate = today.AddDays(ReminderDays[0]); // 30 days out

        // Get active schedules with upcoming due dates
        var schedules = await tenantUow.Repository<MaintenanceSchedule>()
            .GetListAsync(s =>
                s.IsActive &&
                s.NextDueDate.HasValue &&
                s.NextDueDate <= maxDate);

        var overdueCount = 0;
        var upcomingCount = 0;

        foreach (var schedule in schedules)
        {
            var daysUntilDue = schedule.NextDueDate.HasValue
                ? (int)(schedule.NextDueDate.Value - today).TotalDays
                : 0;

            if (schedule.IsOverdue)
            {
                overdueCount++;
                await SendMaintenanceNotificationAsync(tenant.Id.ToString(), schedule, isOverdue: true);
            }
            else if (ShouldSendReminder(daysUntilDue))
            {
                upcomingCount++;
                await SendMaintenanceNotificationAsync(tenant.Id.ToString(), schedule, isOverdue: false, daysUntilDue);
            }
        }

        logger.LogInformation(
            "Processed {OverdueCount} overdue and {UpcomingCount} upcoming maintenance reminders for tenant {TenantName}",
            overdueCount, upcomingCount, tenant.Name);
    }

    private static bool ShouldSendReminder(int daysUntilDue)
    {
        return ReminderDays.Contains(daysUntilDue);
    }

    private async Task SendMaintenanceNotificationAsync(
        string tenantId,
        MaintenanceSchedule schedule,
        bool isOverdue,
        int daysUntilDue = 0)
    {
        var maintenanceTypeName = GetMaintenanceTypeName(schedule.MaintenanceType);
        var truckNumber = schedule.Truck?.Number ?? "Unknown";

        var notification = new NotificationDto
        {
            Title = isOverdue
                ? $"Overdue Maintenance: {maintenanceTypeName}"
                : $"Upcoming Maintenance: {maintenanceTypeName}",
            Message = isOverdue
                ? $"Truck {truckNumber} has overdue {maintenanceTypeName}. Due date was {schedule.NextDueDate:d}."
                : $"Truck {truckNumber} requires {maintenanceTypeName} in {daysUntilDue} days.",
            CreatedDate = DateTime.UtcNow
        };

        await notificationService.BroadcastNotificationAsync(tenantId, notification);
    }

    private static string GetMaintenanceTypeName(MaintenanceType type)
    {
        return type switch
        {
            MaintenanceType.OilChange => "Oil Change",
            MaintenanceType.TireRotation => "Tire Rotation",
            MaintenanceType.TireReplacement => "Tire Replacement",
            MaintenanceType.BrakeInspection => "Brake Inspection",
            MaintenanceType.BrakeReplacement => "Brake Replacement",
            MaintenanceType.AirFilterReplacement => "Air Filter Replacement",
            MaintenanceType.FuelFilterReplacement => "Fuel Filter Replacement",
            MaintenanceType.TransmissionService => "Transmission Service",
            MaintenanceType.CoolantFlush => "Coolant Flush",
            MaintenanceType.BeltInspection => "Belt Inspection",
            MaintenanceType.Battery => "Battery",
            MaintenanceType.AnnualDotInspection => "Annual DOT Inspection",
            MaintenanceType.PreventiveMaintenance => "Preventive Maintenance",
            MaintenanceType.EngineService => "Engine Service",
            MaintenanceType.SuspensionService => "Suspension Service",
            MaintenanceType.ElectricalRepair => "Electrical Repair",
            MaintenanceType.BodyWork => "Body Work",
            MaintenanceType.HvacService => "HVAC Service",
            MaintenanceType.ExhaustSystem => "Exhaust System",
            MaintenanceType.SteeringRepair => "Steering Repair",
            MaintenanceType.Other => "Other Maintenance",
            _ => type.ToString()
        };
    }
}

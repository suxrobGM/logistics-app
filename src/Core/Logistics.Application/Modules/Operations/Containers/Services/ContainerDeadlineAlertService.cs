using Logistics.Application.Abstractions.Notifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Containers.Services;

/// <summary>
/// Sends one notification per container that needs attention, once a day. An overdue box keeps
/// alerting every day with the growing day count and charge - that is the escalation.
/// </summary>
internal sealed class ContainerDeadlineAlertService(
    ITenantUnitOfWork tenantUow,
    INotificationService notificationService) : IContainerDeadlineAlertService
{
    public async Task<int> SendAlertsAsync(DateTime now, CancellationToken ct = default)
    {
        // Pre-filter in SQL to running clocks due by tomorrow; the calculator makes the final call.
        var horizon = DateOnly.FromDateTime(now).AddDays(1);

        var candidates = await tenantUow.Repository<Container>().Query()
            .Where(c =>
                (c.LastFreeDay != null && c.LastFreeDay <= horizon && c.PickedUpAt == null) ||
                (c.EmptyReturnDeadline != null && c.EmptyReturnDeadline <= horizon &&
                 c.ReturnedAt == null && c.StreetTurnedAt == null))
            .ToListAsync(ct);

        var sent = 0;
        foreach (var container in candidates)
        {
            var alert = BuildAlert(container, container.GetFreeTime(now));
            if (alert is null)
            {
                continue;
            }

            await notificationService.SendNotificationAsync(alert.Value.Title, alert.Value.Message);
            sent++;
        }

        return sent;
    }

    internal static (string Title, string Message)? BuildAlert(Container container, ContainerFreeTime freeTime)
    {
        var parts = new List<string>();
        if (freeTime.Demurrage.NeedsAttention)
        {
            parts.Add(Describe(freeTime.Demurrage, "last free day", "demurrage"));
        }

        if (freeTime.PerDiem.NeedsAttention)
        {
            parts.Add(Describe(freeTime.PerDiem, "empty return deadline", "per diem"));
        }

        if (parts.Count == 0)
        {
            return null;
        }

        var overdue = freeTime.Demurrage.Status == FreeTimeStatus.Overdue ||
                      freeTime.PerDiem.Status == FreeTimeStatus.Overdue;
        var title = overdue
            ? $"Container {container.Number} is past its free time"
            : $"Container {container.Number} free time ends soon";

        return (title, string.Join(" ", parts));
    }

    private static string Describe(FreeTimeClock clock, string deadlineName, string chargeName)
    {
        var date = clock.Deadline!.Value.ToString("MMM d");

        if (clock.Status == FreeTimeStatus.Overdue)
        {
            var accrued = clock.RatePerDay is null ? "" : $", {clock.Charge:0.00} {chargeName} accrued";
            return $"{clock.DaysOver} day(s) past the {deadlineName} ({date}){accrued}.";
        }

        var when = clock.DaysRemaining == 0 ? "today" : "tomorrow";
        return $"The {deadlineName} is {when} ({date}).";
    }
}

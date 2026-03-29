using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when a payroll invoice is submitted for approval.
/// Sends in-app notification to managers/approvers.
/// </summary>
internal sealed class PayrollSubmittedNotificationHandler(
    INotificationService notificationService,
    ITelegramNotificationService telegramNotificationService,
    ITenantUnitOfWork tenantUow,
    ILogger<PayrollSubmittedNotificationHandler> logger)
    : IDomainEventHandler<PayrollSubmittedEvent>
{
    public async Task Handle(PayrollSubmittedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Payroll {PayrollId} submitted for approval, sending notifications",
            @event.PayrollId);

        // Send in-app notification for managers
        await notificationService.SendNotificationAsync(
            "Payroll Pending Approval",
            $"Payroll #{@event.PayrollNumber} for {@event.EmployeeName} ({@event.TotalAmount:C}) requires approval");

        // Send Telegram notification to dispatchers/managers
        try
        {
            await telegramNotificationService.SendNotificationAsync(
                tenantUow.GetCurrentTenant().Id,
                "Payroll Pending Approval",
                $"Payroll #{@event.PayrollNumber} for {@event.EmployeeName} ({@event.TotalAmount:C}) requires approval",
                TelegramChatRole.Dispatcher,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send Telegram notification for payroll submission");
        }
    }
}

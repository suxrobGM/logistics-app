using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when a payroll invoice is submitted for approval.
/// Sends in-app notification to managers/approvers.
/// </summary>
internal sealed class PayrollSubmittedNotificationHandler(
    INotificationService notificationService,
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
    }
}

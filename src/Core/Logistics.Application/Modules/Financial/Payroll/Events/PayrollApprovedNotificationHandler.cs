using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Modules.Operations.Notifications.Services;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Financial.Payroll.Events;

/// <summary>Tells the employee their payroll was approved: push, in-app and email.</summary>
internal sealed class PayrollApprovedNotificationHandler(IEventNotifier notifier)
    : INotificationHandler<PayrollApprovedEvent>
{
    public async Task Handle(PayrollApprovedEvent @event, CancellationToken cancellationToken)
    {
        var total = Money.Format(@event.TotalAmount, @event.Currency);
        var data = new Dictionary<string, string>
        {
            ["payrollId"] = @event.PayrollId.ToString(),
            ["type"] = "payroll_approved"
        };

        await notifier.PushAsync("Payroll Approved",
            $"Your payroll #{@event.PayrollNumber} for {total} has been approved", data, @event.EmployeeDeviceToken);
        await notifier.ShowInAppAsync("Payroll Approved",
            $"Payroll #{@event.PayrollNumber} for {@event.EmployeeName} has been approved");
        await notifier.EmailAsync(@event.EmployeeEmail, "Your Payroll Has Been Approved", "PayrollApproved",
            new PayrollApprovedEmailModel
            {
                EmployeeName = @event.EmployeeName,
                PayrollNumber = @event.PayrollNumber,
                TotalAmount = total,
                Currency = @event.Currency,
                PeriodStart = @event.PeriodStart.ToString("MMMM d, yyyy"),
                PeriodEnd = @event.PeriodEnd.ToString("MMMM d, yyyy")
            });
    }
}

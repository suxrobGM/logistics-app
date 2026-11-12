using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Modules.Platform.Notifications.Services;
using Logistics.Domain.Events;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Financial.Payroll.Events;

/// <summary>Tells the employee their payroll was rejected and why: push, in-app and email.</summary>
internal sealed class PayrollRejectedNotificationHandler(IEventNotifier notifier)
    : INotificationHandler<PayrollRejectedEvent>
{
    public async Task Handle(PayrollRejectedEvent @event, CancellationToken cancellationToken)
    {
        var data = new Dictionary<string, string>
        {
            ["payrollId"] = @event.PayrollId.ToString(),
            ["type"] = "payroll_rejected"
        };

        await notifier.PushAsync("Payroll Rejected",
            $"Your payroll #{@event.PayrollNumber} has been rejected", data, @event.EmployeeDeviceToken);
        await notifier.ShowInAppAsync("Payroll Rejected",
            $"Payroll #{@event.PayrollNumber} for {@event.EmployeeName} has been rejected: {@event.RejectionReason}");
        await notifier.EmailAsync(@event.EmployeeEmail, "Your Payroll Has Been Rejected", "PayrollRejected",
            new PayrollRejectedEmailModel
            {
                EmployeeName = @event.EmployeeName,
                PayrollNumber = @event.PayrollNumber,
                RejectionReason = @event.RejectionReason,
                PeriodStart = @event.PeriodStart.ToString("MMMM d, yyyy"),
                PeriodEnd = @event.PeriodEnd.ToString("MMMM d, yyyy")
            });
    }
}

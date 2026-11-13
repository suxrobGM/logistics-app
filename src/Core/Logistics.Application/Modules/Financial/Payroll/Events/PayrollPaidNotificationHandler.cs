using Logistics.Application.Abstractions.Communications.EmailModels;
using Logistics.Application.Modules.Operations.Notifications.Services;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Financial.Payroll.Events;

/// <summary>Tells the employee a payroll payment landed: push, in-app and email.</summary>
internal sealed class PayrollPaidNotificationHandler(IEventNotifier notifier, ITenantUnitOfWork tenantUow)
    : INotificationHandler<PayrollPaidEvent>
{
    public async Task Handle(PayrollPaidEvent @event, CancellationToken cancellationToken)
    {
        var currency = tenantUow.GetCurrentTenant().Currency;
        var payment = Money.Format(@event.PaymentAmount, currency);
        var total = Money.Format(@event.TotalAmount, currency);
        var outstanding = Money.Format(@event.OutstandingAmount, currency);
        var data = new Dictionary<string, string>
        {
            ["payrollId"] = @event.PayrollId.ToString(),
            ["type"] = "payroll_paid"
        };

        var title = @event.IsFullyPaid ? "Payroll Paid" : "Payroll Payment Received";
        var message = @event.IsFullyPaid
            ? $"Your payroll #{@event.PayrollNumber} for {total} has been paid in full"
            : $"Payment of {payment} received for payroll #{@event.PayrollNumber}. Outstanding: {outstanding}";

        await notifier.PushAsync(title, message, data, @event.EmployeeDeviceToken);
        await notifier.ShowInAppAsync(title, message);
        await notifier.EmailAsync(@event.EmployeeEmail, title, "PayrollPaid", new PayrollPaidEmailModel
        {
            EmployeeName = @event.EmployeeName,
            PayrollNumber = @event.PayrollNumber,
            PaymentAmount = payment,
            TotalAmount = total,
            OutstandingAmount = outstanding,
            Currency = currency,
            IsFullyPaid = @event.IsFullyPaid,
            PeriodStart = @event.PeriodStart.ToString("MMMM d, yyyy"),
            PeriodEnd = @event.PeriodEnd.ToString("MMMM d, yyyy")
        });
    }
}

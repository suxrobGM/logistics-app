using Logistics.Application.Modules.Operations.Notifications.Services;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;

namespace Logistics.Application.Modules.Financial.Payroll.Events;

/// <summary>Asks the people who approve payroll to review a submitted one: in-app and the dispatcher chats.</summary>
internal sealed class PayrollSubmittedNotificationHandler(IEventNotifier notifier)
    : INotificationHandler<PayrollSubmittedEvent>
{
    public async Task Handle(PayrollSubmittedEvent @event, CancellationToken cancellationToken)
    {
        const string title = "Payroll Pending Approval";
        var message = $"Payroll #{@event.PayrollNumber} for {@event.EmployeeName} " +
                      $"({Money.Format(@event.TotalAmount, @event.Currency)}) requires approval";

        await notifier.ShowInAppAsync(title, message);
        await notifier.PostToTelegramAsync(title, message, TelegramChatRole.Dispatcher, cancellationToken);
    }
}

using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Abstractions.Email;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Notifications;

namespace Logistics.Application.Modules.Financial.Payroll.Events;

/// <summary>
///     Handles notifications when a payment is recorded against a payroll invoice.
///     Sends push notification and email to employee.
/// </summary>
internal sealed class PayrollPaidNotificationHandler(
    IPushNotificationService pushNotificationService,
    INotificationService notificationService,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ILogger<PayrollPaidNotificationHandler> logger)
    : INotificationHandler<PayrollPaidEvent>
{
    public async Task Handle(PayrollPaidEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Payment of {Amount} recorded for payroll {PayrollId}, sending notifications to employee {EmployeeId}",
            @event.PaymentAmount, @event.PayrollId, @event.EmployeeId);

        var data = new Dictionary<string, string>
        {
            ["payrollId"] = @event.PayrollId.ToString(),
            ["type"] = "payroll_paid"
        };

        var title = @event.IsFullyPaid ? "Payroll Paid" : "Payroll Payment Received";
        var message = @event.IsFullyPaid
            ? $"Your payroll #{@event.PayrollNumber} for {Money.Format(@event.TotalAmount, @event.Currency)} has been paid in full"
            : $"Payment of {Money.Format(@event.PaymentAmount, @event.Currency)} received for payroll #{@event.PayrollNumber}. Outstanding: {Money.Format(@event.OutstandingAmount, @event.Currency)}";

        // Send push notification to employee
        if (!string.IsNullOrEmpty(@event.EmployeeDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                title,
                message,
                @event.EmployeeDeviceToken,
                data);
        }

        // Send in-app notification
        await notificationService.SendNotificationAsync(title, message);

        // Send email to employee
        if (!string.IsNullOrEmpty(@event.EmployeeEmail))
        {
            try
            {
                var emailModel = new PayrollPaidEmailModel
                {
                    EmployeeName = @event.EmployeeName,
                    PayrollNumber = @event.PayrollNumber,
                    PaymentAmount = Money.Format(@event.PaymentAmount, @event.Currency),
                    TotalAmount = Money.Format(@event.TotalAmount, @event.Currency),
                    OutstandingAmount = Money.Format(@event.OutstandingAmount, @event.Currency),
                    Currency = @event.Currency,
                    IsFullyPaid = @event.IsFullyPaid,
                    PeriodStart = @event.PeriodStart.ToString("MMMM d, yyyy"),
                    PeriodEnd = @event.PeriodEnd.ToString("MMMM d, yyyy")
                };

                var htmlBody = await emailTemplateService.RenderAsync("PayrollPaid", emailModel);
                await emailSender.SendEmailAsync(@event.EmployeeEmail, title, htmlBody);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send payroll payment email to {Email}", @event.EmployeeEmail);
            }
        }
    }
}

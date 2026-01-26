using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Services.EmailSender;
using Logistics.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when a payment is recorded against a payroll invoice.
/// Sends push notification and email to employee.
/// </summary>
internal sealed class PayrollPaidNotificationHandler(
    IPushNotificationService pushNotificationService,
    INotificationService notificationService,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ILogger<PayrollPaidNotificationHandler> logger)
    : IDomainEventHandler<PayrollPaidEvent>
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
            ? $"Your payroll #{@event.PayrollNumber} for {@event.TotalAmount:C} has been paid in full"
            : $"Payment of {@event.PaymentAmount:C} received for payroll #{@event.PayrollNumber}. Outstanding: {@event.OutstandingAmount:C}";

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
                    PaymentAmount = @event.PaymentAmount.ToString("C"),
                    TotalAmount = @event.TotalAmount.ToString("C"),
                    OutstandingAmount = @event.OutstandingAmount.ToString("C"),
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

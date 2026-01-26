using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Services.EmailSender;
using Logistics.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when a payroll invoice is rejected.
/// Sends push notification and email to employee with rejection reason.
/// </summary>
internal sealed class PayrollRejectedNotificationHandler(
    IPushNotificationService pushNotificationService,
    INotificationService notificationService,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ILogger<PayrollRejectedNotificationHandler> logger)
    : IDomainEventHandler<PayrollRejectedEvent>
{
    public async Task Handle(PayrollRejectedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Payroll {PayrollId} rejected, sending notifications to employee {EmployeeId}",
            @event.PayrollId, @event.EmployeeId);

        var data = new Dictionary<string, string>
        {
            ["payrollId"] = @event.PayrollId.ToString(),
            ["type"] = "payroll_rejected"
        };

        // Send push notification to employee
        if (!string.IsNullOrEmpty(@event.EmployeeDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Payroll Rejected",
                $"Your payroll #{@event.PayrollNumber} has been rejected",
                @event.EmployeeDeviceToken,
                data);
        }

        // Send in-app notification
        await notificationService.SendNotificationAsync(
            "Payroll Rejected",
            $"Payroll #{@event.PayrollNumber} for {@event.EmployeeName} has been rejected: {@event.RejectionReason}");

        // Send email to employee
        if (!string.IsNullOrEmpty(@event.EmployeeEmail))
        {
            try
            {
                var emailModel = new PayrollRejectedEmailModel
                {
                    EmployeeName = @event.EmployeeName,
                    PayrollNumber = @event.PayrollNumber,
                    RejectionReason = @event.RejectionReason,
                    PeriodStart = @event.PeriodStart.ToString("MMMM d, yyyy"),
                    PeriodEnd = @event.PeriodEnd.ToString("MMMM d, yyyy")
                };

                var htmlBody = await emailTemplateService.RenderAsync("PayrollRejected", emailModel);
                await emailSender.SendEmailAsync(@event.EmployeeEmail, "Your Payroll Has Been Rejected", htmlBody);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send payroll rejection email to {Email}", @event.EmployeeEmail);
            }
        }
    }
}

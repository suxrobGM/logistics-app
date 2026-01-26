using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Services.EmailSender;
using Logistics.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Events;

/// <summary>
/// Handles notifications when a payroll invoice is approved.
/// Sends push notification and email to employee.
/// </summary>
internal sealed class PayrollApprovedNotificationHandler(
    IPushNotificationService pushNotificationService,
    INotificationService notificationService,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ILogger<PayrollApprovedNotificationHandler> logger)
    : IDomainEventHandler<PayrollApprovedEvent>
{
    public async Task Handle(PayrollApprovedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Payroll {PayrollId} approved, sending notifications to employee {EmployeeId}",
            @event.PayrollId, @event.EmployeeId);

        var data = new Dictionary<string, string>
        {
            ["payrollId"] = @event.PayrollId.ToString(),
            ["type"] = "payroll_approved"
        };

        // Send push notification to employee
        if (!string.IsNullOrEmpty(@event.EmployeeDeviceToken))
        {
            await pushNotificationService.SendNotificationAsync(
                "Payroll Approved",
                $"Your payroll #{@event.PayrollNumber} for {@event.TotalAmount:C} has been approved",
                @event.EmployeeDeviceToken,
                data);
        }

        // Send in-app notification
        await notificationService.SendNotificationAsync(
            "Payroll Approved",
            $"Payroll #{@event.PayrollNumber} for {@event.EmployeeName} has been approved");

        // Send email to employee
        if (!string.IsNullOrEmpty(@event.EmployeeEmail))
        {
            try
            {
                var emailModel = new PayrollApprovedEmailModel
                {
                    EmployeeName = @event.EmployeeName,
                    PayrollNumber = @event.PayrollNumber,
                    TotalAmount = @event.TotalAmount.ToString("C"),
                    Currency = @event.Currency,
                    PeriodStart = @event.PeriodStart.ToString("MMMM d, yyyy"),
                    PeriodEnd = @event.PeriodEnd.ToString("MMMM d, yyyy")
                };

                var htmlBody = await emailTemplateService.RenderAsync("PayrollApproved", emailModel);
                await emailSender.SendEmailAsync(@event.EmployeeEmail, "Your Payroll Has Been Approved", htmlBody);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send payroll approval email to {Email}", @event.EmployeeEmail);
            }
        }
    }
}

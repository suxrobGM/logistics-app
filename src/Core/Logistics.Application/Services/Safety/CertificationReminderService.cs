using Logistics.Application.Services.Realtime;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Services;

internal class CertificationReminderService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IRealtimeNotificationService notificationService,
    ILogger<CertificationReminderService> logger)
    : ICertificationReminderService
{
    private static readonly int[] ReminderDays = [90, 60, 30, 14, 7, 1];

    public async Task ProcessExpiringCertificationsAsync(CancellationToken ct = default)
    {
        var tenants = await masterUow.Repository<Tenant>().GetListAsync();

        foreach (var tenant in tenants)
        {
            try
            {
                tenantUow.SetCurrentTenant(tenant);
                await ProcessTenantCertificationsAsync(tenant, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing certifications for tenant {TenantName}", tenant.Name);
            }
        }
    }

    private async Task ProcessTenantCertificationsAsync(Tenant tenant, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var maxDate = today.AddDays(ReminderDays[0]); // 90 days out

        var certifications = await tenantUow.Repository<DriverCertification>()
            .GetListAsync(c =>
                c.Status == CertificationStatus.Active &&
                c.ExpirationDate <= maxDate &&
                c.ExpirationDate >= today);

        var expiredCertifications = await tenantUow.Repository<DriverCertification>()
            .GetListAsync(c =>
                c.Status == CertificationStatus.Active &&
                c.ExpirationDate < today);

        // Mark expired certifications
        foreach (var cert in expiredCertifications)
        {
            cert.Status = CertificationStatus.Expired;
            tenantUow.Repository<DriverCertification>().Update(cert);

            await SendExpirationNotificationAsync(tenant.Id.ToString(), cert, isExpired: true);
        }

        // Send reminders for expiring certifications
        foreach (var cert in certifications)
        {
            var daysUntilExpiration = (cert.ExpirationDate - today).Days;

            if (ShouldSendReminder(daysUntilExpiration))
            {
                // Update status to ExpiringSoon if within 30 days
                if (daysUntilExpiration <= 30 && cert.Status == CertificationStatus.Active)
                {
                    cert.Status = CertificationStatus.ExpiringSoon;
                    tenantUow.Repository<DriverCertification>().Update(cert);
                }

                await SendExpirationNotificationAsync(tenant.Id.ToString(), cert, isExpired: false, daysUntilExpiration);
            }
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Processed {ExpiringCount} expiring and {ExpiredCount} expired certifications for tenant {TenantName}",
            certifications.Count, expiredCertifications.Count, tenant.Name);
    }

    private static bool ShouldSendReminder(int daysUntilExpiration)
    {
        return ReminderDays.Contains(daysUntilExpiration);
    }

    private async Task SendExpirationNotificationAsync(
        string tenantId,
        DriverCertification cert,
        bool isExpired,
        int daysUntilExpiration = 0)
    {
        var certTypeName = GetCertificationTypeName(cert.CertificationType);
        var employeeName = cert.Employee?.GetFullName() ?? "Driver";

        var notification = new NotificationDto
        {
            Title = isExpired
                ? $"Certification Expired: {certTypeName}"
                : $"Certification Expiring Soon: {certTypeName}",
            Message = isExpired
                ? $"{employeeName}'s {certTypeName} has expired."
                : $"{employeeName}'s {certTypeName} expires in {daysUntilExpiration} days.",
            CreatedDate = DateTime.UtcNow
        };

        await notificationService.BroadcastNotificationAsync(tenantId, notification);
    }

    private static string GetCertificationTypeName(CertificationType type)
    {
        return type switch
        {
            CertificationType.Cdl => "CDL",
            CertificationType.MedicalCertificate => "Medical Certificate",
            CertificationType.HazmatEndorsement => "Hazmat Endorsement",
            CertificationType.TwicCard => "TWIC Card",
            CertificationType.TankerEndorsement => "Tanker Endorsement",
            CertificationType.DoublesTriples => "Doubles/Triples Endorsement",
            CertificationType.PassengerEndorsement => "Passenger Endorsement",
            CertificationType.SchoolBusEndorsement => "School Bus Endorsement",
            CertificationType.Eldt => "ELDT",
            CertificationType.DefensiveDriving => "Defensive Driving",
            CertificationType.HazmatTraining => "Hazmat Training",
            _ => type.ToString()
        };
    }
}

using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Compliance.Safety.Services;

/// <summary>Notifies the current tenant about driver licenses approaching expiry.</summary>
public interface ILicenseExpiryReminderService : IApplicationService
{
    Task ProcessLicenseExpiryRemindersAsync(Tenant tenant, CancellationToken ct = default);
}

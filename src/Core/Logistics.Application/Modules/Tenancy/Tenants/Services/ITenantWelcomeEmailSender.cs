using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Tenancy.Tenants.Services;

public interface ITenantWelcomeEmailSender : IApplicationService
{
    /// <summary>Emails the owner a login link, plus <paramref name="temporaryPassword" /> when their account is new.</summary>
    Task SendAsync(User owner, Tenant tenant, string? temporaryPassword = null);
}

using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.PaymentLinks.Services;

public interface IPaymentLinkService : IApplicationService
{
    /// <summary>Adds a new link to the invoice's pay page. The caller saves.</summary>
    Task<PaymentLink> CreateAsync(Guid invoiceId, int expirationDays, Guid? createdByUserId, CancellationToken ct = default);

    /// <summary>The customer-portal pay page for <paramref name="link" /> in the current tenant.</summary>
    string GetUrl(PaymentLink link);

    /// <summary>The link for <paramref name="token" /> in the current tenant, when it is still valid.</summary>
    Task<Result<PaymentLink>> FindValidAsync(string token, CancellationToken ct = default);
}

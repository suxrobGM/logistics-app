using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Application.Abstractions.Options;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Financial.PaymentLinks.Services;

internal sealed class PaymentLinkService(
    ITenantUnitOfWork tenantUow,
    IOptions<CustomerPortalOptions> portalOptions) : IPaymentLinkService
{
    public async Task<PaymentLink> CreateAsync(
        Guid invoiceId, int expirationDays, Guid? createdByUserId, CancellationToken ct = default)
    {
        var link = new PaymentLink
        {
            Token = TokenGenerator.GenerateSecureToken(),
            InvoiceId = invoiceId,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            IsActive = true,
            CreatedByUserId = createdByUserId ?? Guid.Empty
        };

        await tenantUow.Repository<PaymentLink>().AddAsync(link, ct);
        return link;
    }

    public string GetUrl(PaymentLink link) =>
        $"{portalOptions.Value.BaseUrl}/pay/{tenantUow.GetCurrentTenant().Id}/{link.Token}";

    public async Task<Result<PaymentLink>> FindValidAsync(string token, CancellationToken ct = default)
    {
        var link = await tenantUow.Repository<PaymentLink>().GetAsync(p => p.Token == token, ct);
        if (link is null)
        {
            return Result<PaymentLink>.Fail("Invalid payment link.");
        }

        return link.IsValid
            ? Result<PaymentLink>.Ok(link)
            : Result<PaymentLink>.Fail("This payment link has expired or been revoked.");
    }
}

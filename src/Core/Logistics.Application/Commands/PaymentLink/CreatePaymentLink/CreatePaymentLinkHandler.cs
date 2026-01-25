using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Options;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentLinkHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService,
    IOptions<CustomerPortalOptions> portalOptions,
    ILogger<CreatePaymentLinkHandler> logger)
    : IAppRequestHandler<CreatePaymentLinkCommand, Result<PaymentLinkDto>>
{
    public async Task<Result<PaymentLinkDto>> Handle(CreatePaymentLinkCommand req, CancellationToken ct)
    {
        var currentUserId = currentUserService.GetUserId();

        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<PaymentLinkDto>.Fail("Invoice not found.");
        }

        // Generate a secure random token
        var token = TokenGenerator.GenerateSecureToken();

        var paymentLink = new PaymentLink
        {
            Token = token,
            InvoiceId = invoice.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(req.ExpirationDays),
            IsActive = true,
            CreatedByUserId = currentUserId ?? Guid.Empty
        };

        await tenantUow.Repository<PaymentLink>().AddAsync(paymentLink, ct);
        await tenantUow.SaveChangesAsync(ct);

        // Build the full URL
        var tenant = tenantUow.GetCurrentTenant();
        var baseUrl = portalOptions.Value.BaseUrl;
        var fullUrl = $"{baseUrl}/pay/{tenant.Id}/{token}";

        logger.LogInformation(
            "Created payment link {LinkId} for invoice {InvoiceId}, expires at {ExpiresAt}",
            paymentLink.Id, invoice.Id, paymentLink.ExpiresAt);

        var dto = paymentLink.ToDto(baseUrl);
        dto.Url = fullUrl;

        return Result<PaymentLinkDto>.Ok(dto);
    }
}

using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Financial.PaymentLinks.Commands;

internal sealed class RecordPaymentLinkAccessHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    ILogger<RecordPaymentLinkAccessHandler> logger)
    : IAppRequestHandler<RecordPaymentLinkAccessCommand, Result>
{
    public async Task<Result> Handle(RecordPaymentLinkAccessCommand req, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);
        if (tenant is null)
        {
            return Result.Fail("Tenant not found.");
        }

        tenantUow.SetCurrentTenant(tenant);

        var paymentLink = await tenantUow.Repository<PaymentLink>()
            .GetByIdAsync(req.PaymentLinkId, ct);
        if (paymentLink is null)
        {
            logger.LogWarning(
                "RecordPaymentLinkAccess: payment link {LinkId} not found for tenant {TenantId}.",
                req.PaymentLinkId, req.TenantId);
            return Result.Fail("Payment link not found.");
        }

        paymentLink.RecordAccess();
        tenantUow.Repository<PaymentLink>().Update(paymentLink);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

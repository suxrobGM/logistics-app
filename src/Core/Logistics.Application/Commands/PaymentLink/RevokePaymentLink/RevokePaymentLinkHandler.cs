using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class RevokePaymentLinkHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<RevokePaymentLinkHandler> logger)
    : IAppRequestHandler<RevokePaymentLinkCommand, Result>
{
    public async Task<Result> Handle(RevokePaymentLinkCommand req, CancellationToken ct)
    {
        var paymentLink = await tenantUow.Repository<PaymentLink>().GetByIdAsync(req.PaymentLinkId, ct);
        if (paymentLink is null)
        {
            return Result.Fail("Payment link not found.");
        }

        paymentLink.Revoke();

        tenantUow.Repository<PaymentLink>().Update(paymentLink);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Revoked payment link {LinkId}", paymentLink.Id);

        return Result.Ok();
    }
}

using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeletePaymentMethodHandler(
    ITenantUnitOfWork tenantUow,
    IStripePaymentService stripePaymentService,
    ILogger<DeletePaymentMethodHandler> logger) : IAppRequestHandler<DeletePaymentMethodCommand, Result>
{
    public async Task<Result> Handle(
        DeletePaymentMethodCommand req, CancellationToken ct)
    {
        var paymentMethod = await tenantUow.Repository<PaymentMethod>().GetByIdAsync(req.Id, ct);

        if (paymentMethod is null)
        {
            return Result.Fail($"Could not find a payment with ID {req.Id}");
        }

        await stripePaymentService.RemovePaymentMethodAsync(paymentMethod);
        tenantUow.Repository<PaymentMethod>().Delete(paymentMethod);
        await tenantUow.SaveChangesAsync(ct);
        logger.LogInformation("Deleted payment method with ID {Id}", paymentMethod.Id);
        return Result.Ok();
    }
}

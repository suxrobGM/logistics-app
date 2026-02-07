using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class SetDefaultPaymentMethodHandler(
    ITenantUnitOfWork tenantUow,
    IStripePaymentService stripePaymentService,
    ILogger<SetDefaultPaymentMethodHandler> logger) : IAppRequestHandler<SetDefaultPaymentMethodCommand, Result>
{
    public async Task<Result> Handle(
        SetDefaultPaymentMethodCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        // Load all payment methods for the tenant
        var paymentMethods = await tenantUow.Repository<PaymentMethod>().GetListAsync(ct: ct);

        if (paymentMethods.Count == 0)
        {
            return Result.Fail($"No payment methods found for tenant {tenant.Id}");
        }

        // Set all payment methods to not default
        foreach (var paymentMethod in paymentMethods)
        {
            paymentMethod.IsDefault = false;
        }

        // Set the specified payment method to default using the already loaded entity
        var paymentMethodToSetDefault = paymentMethods.FirstOrDefault(i => i.Id == req.PaymentMethodId);
        if (paymentMethodToSetDefault is null)
        {
            return Result.Fail($"Payment method with ID {req.PaymentMethodId} not found for tenant {tenant.Id}");
        }

        paymentMethodToSetDefault.IsDefault = true;

        await stripePaymentService.SetDefaultPaymentMethodAsync(paymentMethodToSetDefault, tenant);
        tenantUow.Repository<PaymentMethod>().Update(paymentMethodToSetDefault);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Set payment method with ID {Id} as default for tenant {TenantId}",
            paymentMethodToSetDefault.Id, tenant.Id);
        return Result.Ok();
    }
}

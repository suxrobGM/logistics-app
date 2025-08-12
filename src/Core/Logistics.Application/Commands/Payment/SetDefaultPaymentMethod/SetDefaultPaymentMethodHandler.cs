using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class SetDefaultPaymentMethodHandler : RequestHandler<SetDefaultPaymentMethodCommand, Result>
{
    private readonly ILogger<SetDefaultPaymentMethodHandler> _logger;
    private readonly IStripeService _stripeService;
    private readonly ITenantUnitOfWork _tenantUow;

    public SetDefaultPaymentMethodHandler(
        ITenantUnitOfWork tenantUow,
        IStripeService stripeService,
        ILogger<SetDefaultPaymentMethodHandler> logger)
    {
        _tenantUow = tenantUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        SetDefaultPaymentMethodCommand req, CancellationToken ct)
    {
        var tenant = _tenantUow.GetCurrentTenant();

        // Load all payment methods for the tenant
        var paymentMethods = await _tenantUow.Repository<PaymentMethod>().GetListAsync();

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

        await _stripeService.SetDefaultPaymentMethodAsync(paymentMethodToSetDefault, tenant);
        _tenantUow.Repository<PaymentMethod>().Update(paymentMethodToSetDefault);
        await _tenantUow.SaveChangesAsync();

        _logger.LogInformation("Set payment method with ID {Id} as default for tenant {TenantId}",
            paymentMethodToSetDefault.Id, tenant.Id);
        return Result.Succeed();
    }
}

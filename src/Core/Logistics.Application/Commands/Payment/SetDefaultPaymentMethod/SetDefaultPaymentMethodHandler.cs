using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class SetDefaultPaymentMethodHandler : RequestHandler<SetDefaultPaymentMethodCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly IStripeService _stripeService;
    private readonly ILogger<SetDefaultPaymentMethodHandler> _logger;

    public SetDefaultPaymentMethodHandler(
        IMasterUnityOfWork masterUow,
        IStripeService stripeService,
        ILogger<SetDefaultPaymentMethodHandler> logger)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        SetDefaultPaymentMethodCommand req, CancellationToken cancellationToken)
    {
        // var tenant = await _masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId);
        //
        // if (tenant is null)
        // {
        //     return Result.Fail($"Tenant with id {req.TenantId} not found");
        // }
        
        // Load all payment methods for the tenant
        var paymentMethods = await _masterUow.Repository<PaymentMethod>().GetListAsync(i => i.TenantId == req.TenantId);

        if (paymentMethods.Count == 0)
        {
            return Result.Fail($"No payment methods found for tenant {req.TenantId}");
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
            return Result.Fail($"Payment method with ID {req.PaymentMethodId} not found for tenant {req.TenantId}");
        }
        
        paymentMethodToSetDefault.IsDefault = true;

        await _stripeService.SetDefaultPaymentMethodAsync(paymentMethodToSetDefault);
        _masterUow.Repository<PaymentMethod>().Update(paymentMethodToSetDefault);
        await _masterUow.SaveChangesAsync();
        
        _logger.LogInformation("Set payment method with ID {Id} as default for tenant {TenantId}", 
            paymentMethodToSetDefault.Id, paymentMethodToSetDefault.TenantId);
        return Result.Succeed();
    }
}

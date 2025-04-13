using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeletePaymentMethodHandler : RequestHandler<DeletePaymentMethodCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly IStripeService _stripeService;
    private readonly ILogger<DeletePaymentMethodHandler> _logger;

    public DeletePaymentMethodHandler(
        IMasterUnityOfWork masterUow,
        IStripeService stripeService,
        ILogger<DeletePaymentMethodHandler> logger)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    protected override async Task<Result> HandleValidated(
        DeletePaymentMethodCommand req, CancellationToken cancellationToken)
    {
        var paymentMethod = await _masterUow.Repository<PaymentMethod>().GetAsync(i => i.Id == req.Id && i.TenantId == req.TenantId);

        if (paymentMethod is null)
        {
            return Result.Fail($"Could not find a payment with ID {req.Id} for tenant {req.TenantId}");
        }

        await _stripeService.RemovePaymentMethodAsync(paymentMethod);
        _masterUow.Repository<PaymentMethod>().Delete(paymentMethod);
        await _masterUow.SaveChangesAsync();
        _logger.LogInformation("Deleted payment method with ID {Id} from tenant {TenantId}", paymentMethod.Id, paymentMethod.TenantId);
        return Result.Succeed();
    }
}

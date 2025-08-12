using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeletePaymentMethodHandler : IAppRequestHandler<DeletePaymentMethodCommand, Result>
{
    private readonly ILogger<DeletePaymentMethodHandler> _logger;
    private readonly IStripeService _stripeService;
    private readonly ITenantUnitOfWork _tenantUow;

    public DeletePaymentMethodHandler(
        ITenantUnitOfWork tenantUow,
        IStripeService stripeService,
        ILogger<DeletePaymentMethodHandler> logger)
    {
        _tenantUow = tenantUow;
        _stripeService = stripeService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeletePaymentMethodCommand req, CancellationToken ct)
    {
        var paymentMethod = await _tenantUow.Repository<PaymentMethod>().GetByIdAsync(req.Id);

        if (paymentMethod is null)
        {
            return Result.Fail($"Could not find a payment with ID {req.Id}");
        }

        await _stripeService.RemovePaymentMethodAsync(paymentMethod);
        _tenantUow.Repository<PaymentMethod>().Delete(paymentMethod);
        await _tenantUow.SaveChangesAsync();
        _logger.LogInformation("Deleted payment method with ID {Id}", paymentMethod.Id);
        return Result.Succeed();
    }
}

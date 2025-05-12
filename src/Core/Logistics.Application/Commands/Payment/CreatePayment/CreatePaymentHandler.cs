using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentHandler : RequestHandler<CreatePaymentCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public CreatePaymentHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        CreatePaymentCommand req, CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            Amount = req.Amount,
            Method = req.Method,
            BillingAddress = req.BillingAddress ?? Address.NullAddress,
        };
        
        await _tenantUow.Repository<Payment>().AddAsync(payment);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}

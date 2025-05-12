using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdatePaymentHandler : RequestHandler<UpdatePaymentCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdatePaymentHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdatePaymentCommand req, CancellationToken cancellationToken)
    {
        var payment = await _tenantUow.Repository<Payment>().GetByIdAsync(req.Id);

        if (payment is null)
        {
            return Result.Fail($"Could not find a payment with ID '{req.Id}'");
        }
        
        if (req.Method.HasValue && payment.Method != req.Method)
        {
            payment.Method = req.Method.Value;
        }
        if (req.Status.HasValue && payment.Status != req.Status)
        {
            payment.Status = req.Status.Value;
        }
        if (req.Amount.HasValue && payment.Amount != req.Amount)
        {
            payment.Amount = req.Amount.Value;
        }
        if (req.BillingAddress != null && payment.BillingAddress != req.BillingAddress)
        {
            payment.BillingAddress = req.BillingAddress;
        }
        
        _tenantUow.Repository<Payment>().Update(payment);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}

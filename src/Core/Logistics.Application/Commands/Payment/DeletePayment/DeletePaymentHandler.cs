using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeletePaymentHandler : RequestHandler<DeletePaymentCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public DeletePaymentHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        DeletePaymentCommand req, CancellationToken ct)
    {
        var payment = await _tenantUow.Repository<Payment>().GetByIdAsync(req.Id);

        if (payment is null) return Result.Fail($"Could not find a payment with ID {req.Id}");


        _tenantUow.Repository<Payment>().Delete(payment);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}

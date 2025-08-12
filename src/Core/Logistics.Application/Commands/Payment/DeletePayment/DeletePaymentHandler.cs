using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeletePaymentHandler : RequestHandler<DeletePaymentCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public DeletePaymentHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public override async Task<Result> Handle(
        DeletePaymentCommand req, CancellationToken ct)
    {
        var payment = await _tenantUow.Repository<Payment>().GetByIdAsync(req.Id);

        if (payment is null)
        {
            return Result.Fail($"Could not find a payment with ID {req.Id}");
        }


        _tenantUow.Repository<Payment>().Delete(payment);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}

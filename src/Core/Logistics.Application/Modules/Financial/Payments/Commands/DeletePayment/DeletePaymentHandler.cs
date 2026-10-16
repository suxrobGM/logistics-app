using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class DeletePaymentHandler(ITenantUnitOfWork tenantUow) : IRequestHandler<DeletePaymentCommand, Result>
{
    public async Task<Result> Handle(DeletePaymentCommand req, CancellationToken ct)
    {
        var payment = await tenantUow.Repository<Payment>().GetByIdAsync(req.Id, ct);
        if (payment is null)
        {
            return Result.Fail($"Could not find a {EntityName.Humanize<Payment>()} with ID '{req.Id}'");
        }

        var invoice = await tenantUow.Repository<Invoice>().GetAsync(i => i.Payments.Any(p => p.Id == payment.Id), ct);
        invoice?.RemovePayment(payment, DateTime.UtcNow);

        tenantUow.Repository<Payment>().Delete(payment);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

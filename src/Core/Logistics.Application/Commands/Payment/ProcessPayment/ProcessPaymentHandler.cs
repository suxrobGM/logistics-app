using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class ProcessPaymentHandler : IAppRequestHandler<ProcessPaymentCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public ProcessPaymentHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result> Handle(
        ProcessPaymentCommand req, CancellationToken ct)
    {
        var payment = await _tenantUow.Repository<Payment>().GetByIdAsync(req.PaymentId);

        if (payment is null)
        {
            return Result.Fail($"Could not find a payment with ID '{req.PaymentId}'");
        }

        // TODO: Add payment verification from external provider
        payment.Status = PaymentStatus.Paid;
        _tenantUow.Repository<Payment>().Update(payment);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}

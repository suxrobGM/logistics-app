using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentHandler : IAppRequestHandler<GetPaymentQuery, Result<PaymentDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetPaymentHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result<PaymentDto>> Handle(
        GetPaymentQuery req, CancellationToken ct)
    {
        var paymentEntity = await _tenantUow.Repository<Payment>().GetByIdAsync(req.Id);

        if (paymentEntity is null)
        {
            return Result<PaymentDto>.Fail($"Could not find a payment with ID {req.Id}");
        }

        var paymentDto = paymentEntity.ToDto();
        return Result<PaymentDto>.Succeed(paymentDto);
    }
}

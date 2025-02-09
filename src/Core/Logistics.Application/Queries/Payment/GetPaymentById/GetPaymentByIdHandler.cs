using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentByIdHandler : RequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetPaymentByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<PaymentDto>> HandleValidated(
        GetPaymentByIdQuery req, CancellationToken cancellationToken)
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

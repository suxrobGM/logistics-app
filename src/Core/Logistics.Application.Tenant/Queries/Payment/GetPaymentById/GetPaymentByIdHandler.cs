using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPaymentByIdHandler : RequestHandler<GetPaymentByIdQuery, ResponseResult<PaymentDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetPaymentByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult<PaymentDto>> HandleValidated(
        GetPaymentByIdQuery req, CancellationToken cancellationToken)
    {
        var paymentEntity = await _tenantUow.Repository<Payment>().GetByIdAsync(req.Id);

        if (paymentEntity is null)
        {
            return ResponseResult<PaymentDto>.CreateError($"Could not find a payment with ID {req.Id}");
        }

        var paymentDto = paymentEntity.ToDto();
        return ResponseResult<PaymentDto>.CreateSuccess(paymentDto);
    }
}

using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPaymentByIdHandler : RequestHandler<GetPaymentByIdQuery, ResponseResult<PaymentDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetPaymentByIdHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<PaymentDto>> HandleValidated(
        GetPaymentByIdQuery req, CancellationToken cancellationToken)
    {
        var paymentEntity = await _tenantRepository.GetAsync<Payment>(req.Id);
        
        if (paymentEntity is null)
            return ResponseResult<PaymentDto>.CreateError($"Could not find a payment with ID {req.Id}");

        var paymentDto = paymentEntity.ToDto();
        return ResponseResult<PaymentDto>.CreateSuccess(paymentDto);
    }
}

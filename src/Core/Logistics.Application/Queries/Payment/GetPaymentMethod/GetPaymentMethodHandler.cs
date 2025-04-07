using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentMethodHandler : RequestHandler<GetPaymentMethodQuery, Result<PaymentMethodDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetPaymentMethodHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result<PaymentMethodDto>> HandleValidated(
        GetPaymentMethodQuery req, CancellationToken cancellationToken)
    {
        var paymentMethodEntity = await _masterUow.Repository<PaymentMethod>().GetAsync(i => i.Id == req.Id && i.TenantId == req.TenantId);

        if (paymentMethodEntity is null)
        {
            return Result<PaymentMethodDto>.Fail($"Could not find a payment method with ID {req.Id} for tenant {req.TenantId}");
        }

        var paymentMethodDto = paymentMethodEntity.ToDto();
        return Result<PaymentMethodDto>.Succeed(paymentMethodDto);
    }
}

using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentMethodHandler : RequestHandler<GetPaymentMethodQuery, Result<PaymentMethodDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetPaymentMethodHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<PaymentMethodDto>> HandleValidated(
        GetPaymentMethodQuery req, CancellationToken ct)
    {
        var paymentMethodEntity = await _tenantUow.Repository<PaymentMethod>().GetByIdAsync(req.Id);

        if (paymentMethodEntity is null)
            return Result<PaymentMethodDto>.Fail($"Could not find a payment method with ID {req.Id}");

        var paymentMethodDto = paymentMethodEntity.ToDto();
        return Result<PaymentMethodDto>.Succeed(paymentMethodDto);
    }
}

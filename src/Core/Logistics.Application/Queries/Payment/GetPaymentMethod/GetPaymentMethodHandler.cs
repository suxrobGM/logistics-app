using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentMethodHandler : IAppRequestHandler<GetPaymentMethodQuery, Result<PaymentMethodDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetPaymentMethodHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result<PaymentMethodDto>> Handle(
        GetPaymentMethodQuery req, CancellationToken ct)
    {
        var paymentMethodEntity = await _tenantUow.Repository<PaymentMethod>().GetByIdAsync(req.Id);

        if (paymentMethodEntity is null)
        {
            return Result<PaymentMethodDto>.Fail($"Could not find a payment method with ID {req.Id}");
        }

        var paymentMethodDto = paymentMethodEntity.ToDto();
        return Result<PaymentMethodDto>.Succeed(paymentMethodDto);
    }
}

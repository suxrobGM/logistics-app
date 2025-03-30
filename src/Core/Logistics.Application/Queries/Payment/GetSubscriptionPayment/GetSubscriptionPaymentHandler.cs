using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionPaymentHandler : RequestHandler<GetSubscriptionPaymentQuery, Result<SubscriptionPaymentDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetSubscriptionPaymentHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result<SubscriptionPaymentDto>> HandleValidated(
        GetSubscriptionPaymentQuery req, CancellationToken cancellationToken)
    {
        var paymentEntity = await _masterUow.Repository<SubscriptionPayment>()
            .GetAsync(i => i.Id == req.PaymentId && i.SubscriptionId == req.SubscriptionId);

        if (paymentEntity is null)
        {
            return Result<SubscriptionPaymentDto>.Fail($"Could not find a payment with ID {req.PaymentId}");
        }

        var paymentDto = paymentEntity.ToDto();
        return Result<SubscriptionPaymentDto>.Succeed(paymentDto);
    }
}

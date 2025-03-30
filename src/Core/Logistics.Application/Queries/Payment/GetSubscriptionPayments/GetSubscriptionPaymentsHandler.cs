using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetSubscriptionPaymentsHandler : RequestHandler<GetSubscriptionPaymentsQuery, PagedResult<SubscriptionPaymentDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetSubscriptionPaymentsHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<PagedResult<SubscriptionPaymentDto>> HandleValidated(
        GetSubscriptionPaymentsQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _masterUow.Repository<SubscriptionPayment>().CountAsync();
        var specification = new GetSubscriptionPaymentsById(req.SubscriptionId, req.OrderBy, req.Page, req.PageSize);
        
        var payments = _masterUow.Repository<SubscriptionPayment>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();
        
        return PagedResult<SubscriptionPaymentDto>.Succeed(payments, totalItems, req.PageSize);
    }
}

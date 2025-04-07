using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentMethodsHandler : RequestHandler<GetPaymentMethodsQuery, Result<PaymentMethodDto[]>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetPaymentMethodsHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override Task<Result<PaymentMethodDto[]>> HandleValidated(
        GetPaymentMethodsQuery req, 
        CancellationToken cancellationToken)
    {
        var specification = new GetPaymentMethodsByTenant(req.TenantId!, req.OrderBy);
        
        var payments = _masterUow.Repository<PaymentMethod>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();
        
        return Task.FromResult(Result<PaymentMethodDto[]>.Succeed(payments));
    }
}

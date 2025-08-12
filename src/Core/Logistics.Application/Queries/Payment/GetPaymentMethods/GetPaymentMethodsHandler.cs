using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentMethodsHandler : RequestHandler<GetPaymentMethodsQuery, Result<PaymentMethodDto[]>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetPaymentMethodsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override Task<Result<PaymentMethodDto[]>> HandleValidated(
        GetPaymentMethodsQuery req,
        CancellationToken ct)
    {
        var specification = new GetTenantPaymentMethods(req.OrderBy);

        var payments = _tenantUow.Repository<PaymentMethod>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(Result<PaymentMethodDto[]>.Succeed(payments));
    }
}

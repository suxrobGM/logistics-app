using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPaymentMethodsHandler : IAppRequestHandler<GetPaymentMethodsQuery, Result<PaymentMethodDto[]>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetPaymentMethodsHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public Task<Result<PaymentMethodDto[]>> Handle(GetPaymentMethodsQuery req, CancellationToken ct)
    {
        var specification = new GetTenantPaymentMethods(req.OrderBy);

        var payments = _tenantUow.Repository<PaymentMethod>()
            .ApplySpecification(specification)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(Result<PaymentMethodDto[]>.Ok(payments));
    }
}

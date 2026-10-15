using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Queries;

internal sealed class GetCustomerDetentionTermsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetCustomerDetentionTermsQuery, Result<DetentionTermsDto>>
{
    public async Task<Result<DetentionTermsDto>> Handle(GetCustomerDetentionTermsQuery req, CancellationToken ct)
    {
        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId, ct);
        return customer is null
            ? Result<DetentionTermsDto>.Fail($"Could not find a customer with ID '{req.CustomerId}'")
            : Result<DetentionTermsDto>.Ok(new DetentionTermsDto
            {
                FreeMinutes = customer.DetentionFreeMinutes,
                RatePerHour = customer.DetentionRatePerHour
            });
    }
}

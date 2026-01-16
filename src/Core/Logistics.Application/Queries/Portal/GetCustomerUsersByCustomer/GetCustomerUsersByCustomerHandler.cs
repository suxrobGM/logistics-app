using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetCustomerUsersByCustomerHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetCustomerUsersByCustomerQuery, Result<IEnumerable<CustomerUserDto>>>
{
    public async Task<Result<IEnumerable<CustomerUserDto>>> Handle(
        GetCustomerUsersByCustomerQuery req,
        CancellationToken ct)
    {
        var customerUsers = await tenantUow.Repository<CustomerUser>().Query()
            .Where(cu => cu.CustomerId == req.CustomerId)
            .OrderByDescending(cu => cu.CreatedAt)
            .ToListAsync(ct);

        var dtos = customerUsers.Select(cu => cu.ToDto());

        return Result<IEnumerable<CustomerUserDto>>.Ok(dtos);
    }
}

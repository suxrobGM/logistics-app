using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetCustomerUserHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetCustomerUserQuery, Result<CustomerUserDto>>
{
    public async Task<Result<CustomerUserDto>> Handle(
        GetCustomerUserQuery req,
        CancellationToken ct)
    {
        var customerUser = await tenantUow.Repository<CustomerUser>().Query()
            .Include(cu => cu.Customer)
            .Where(cu => cu.UserId == req.UserId && cu.IsActive)
            .FirstOrDefaultAsync(ct);

        if (customerUser == null)
        {
            return Result<CustomerUserDto>.Fail("Customer user not found or inactive.");
        }

        return Result<CustomerUserDto>.Ok(customerUser.ToDto());
    }
}

using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetCustomerUserByIdHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetCustomerUserByIdQuery, Result<CustomerUserDto>>
{
    public async Task<Result<CustomerUserDto>> Handle(
        GetCustomerUserByIdQuery req,
        CancellationToken ct)
    {
        var customerUser = await tenantUow.Repository<CustomerUser>().Query()
            .Include(cu => cu.Customer)
            .Where(cu => cu.Id == req.Id)
            .FirstOrDefaultAsync(ct);

        if (customerUser == null)
        {
            return Result<CustomerUserDto>.Fail("Customer user not found.");
        }

        return Result<CustomerUserDto>.Ok(customerUser.ToDto());
    }
}

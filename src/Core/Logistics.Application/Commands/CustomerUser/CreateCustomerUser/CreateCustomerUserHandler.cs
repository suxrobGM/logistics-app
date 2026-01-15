using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateCustomerUserHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateCustomerUserCommand, Result<CustomerUserDto>>
{
    public async Task<Result<CustomerUserDto>> Handle(CreateCustomerUserCommand req, CancellationToken ct)
    {
        // Verify the customer exists
        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId, ct);
        if (customer is null)
        {
            return Result<CustomerUserDto>.Fail($"Customer with ID '{req.CustomerId}' not found.");
        }

        // Check if a CustomerUser already exists for this user and customer
        var existingCustomerUser = await tenantUow.Repository<CustomerUser>()
            .GetAsync(cu => cu.UserId == req.UserId && cu.CustomerId == req.CustomerId, ct);

        if (existingCustomerUser is not null)
        {
            return Result<CustomerUserDto>.Fail("A customer user already exists for this user and customer.");
        }

        var customerUser = new CustomerUser
        {
            UserId = req.UserId,
            CustomerId = req.CustomerId,
            Email = req.Email,
            DisplayName = req.DisplayName,
            IsActive = true,
            Customer = customer
        };

        await tenantUow.Repository<CustomerUser>().AddAsync(customerUser, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<CustomerUserDto>.Ok(customerUser.ToDto());
    }
}

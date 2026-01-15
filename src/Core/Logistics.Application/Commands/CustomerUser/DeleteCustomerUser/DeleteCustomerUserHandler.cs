using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteCustomerUserHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteCustomerUserCommand, Result>
{
    public async Task<Result> Handle(DeleteCustomerUserCommand req, CancellationToken ct)
    {
        var customerUser = await tenantUow.Repository<CustomerUser>().GetByIdAsync(req.Id, ct);

        if (customerUser is null)
        {
            return Result.Fail($"Customer user with ID '{req.Id}' not found.");
        }

        tenantUow.Repository<CustomerUser>().Delete(customerUser);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

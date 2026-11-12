using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

internal sealed class SetCustomerUpdatePreferencesHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<SetCustomerUpdatePreferencesCommand, Result>
{
    public async Task<Result> Handle(SetCustomerUpdatePreferencesCommand req, CancellationToken ct)
    {
        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId, ct);
        if (customer is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.CustomerId}'", ErrorCodes.NotFound);
        }

        customer.UpdatePreferences = new CustomerUpdatePreferences
        {
            Dispatched = req.Dispatched,
            PickedUp = req.PickedUp,
            Delivered = req.Delivered,
            Late = req.Late
        };
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

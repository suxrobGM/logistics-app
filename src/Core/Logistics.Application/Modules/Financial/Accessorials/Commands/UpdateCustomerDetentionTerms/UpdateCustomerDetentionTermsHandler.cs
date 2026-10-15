using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

internal sealed class UpdateCustomerDetentionTermsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<UpdateCustomerDetentionTermsCommand, Result>
{
    public async Task<Result> Handle(UpdateCustomerDetentionTermsCommand req, CancellationToken ct)
    {
        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.CustomerId, ct);
        if (customer is null)
        {
            return Result.Fail($"Could not find a customer with ID '{req.CustomerId}'");
        }

        customer.DetentionFreeMinutes = req.FreeMinutes;
        customer.DetentionRatePerHour = req.RatePerHour;
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

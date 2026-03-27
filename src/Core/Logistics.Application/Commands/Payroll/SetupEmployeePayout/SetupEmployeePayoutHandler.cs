using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class SetupEmployeePayoutHandler(
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<SetupEmployeePayoutHandler> logger)
    : IAppRequestHandler<SetupEmployeePayoutCommand, Result>
{
    public async Task<Result> Handle(SetupEmployeePayoutCommand req, CancellationToken ct)
    {
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId, ct);

        if (employee is null)
        {
            return Result.Fail($"Could not find an employee with ID '{req.EmployeeId}'");
        }

        if (!string.IsNullOrEmpty(employee.StripeConnectedAccountId))
        {
            return Result.Ok(); // Already set up
        }

        try
        {
            var account = await stripeConnectService.CreateEmployeeConnectedAccountAsync(employee);
            employee.StripeConnectedAccountId = account.Id;
            tenantUow.Repository<Employee>().Update(employee);
            await tenantUow.SaveChangesAsync(ct);

            logger.LogInformation(
                "Created Stripe Connect account {AccountId} for employee {EmployeeId}",
                account.Id, employee.Id);

            return Result.Ok();
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogError(ex, "Failed to create Stripe Connect account for employee {EmployeeId}", employee.Id);
            return Result.Fail($"Failed to create payout account: {ex.Message}");
        }
    }
}

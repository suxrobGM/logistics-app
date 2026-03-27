using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Queries;

internal sealed class GetEmployeePayoutOnboardingLinkHandler(
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<GetEmployeePayoutOnboardingLinkHandler> logger)
    : IAppRequestHandler<GetEmployeePayoutOnboardingLinkQuery, Result<EmployeePayoutOnboardingLinkDto>>
{
    public async Task<Result<EmployeePayoutOnboardingLinkDto>> Handle(
        GetEmployeePayoutOnboardingLinkQuery req, CancellationToken ct)
    {
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId, ct);

        if (employee is null)
        {
            return Result<EmployeePayoutOnboardingLinkDto>.Fail(
                $"Could not find an employee with ID '{req.EmployeeId}'");
        }

        if (string.IsNullOrEmpty(employee.StripeConnectedAccountId))
        {
            return Result<EmployeePayoutOnboardingLinkDto>.Fail(
                "Employee does not have a payout account. Set up the payout account first.");
        }

        try
        {
            var accountLink = await stripeConnectService.CreateEmployeeAccountLinkAsync(
                employee.StripeConnectedAccountId, req.ReturnUrl, req.RefreshUrl);

            return Result<EmployeePayoutOnboardingLinkDto>.Ok(
                new EmployeePayoutOnboardingLinkDto { Url = accountLink.Url });
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogError(ex, "Failed to create onboarding link for employee {EmployeeId}", employee.Id);
            return Result<EmployeePayoutOnboardingLinkDto>.Fail(
                $"Failed to create onboarding link: {ex.Message}");
        }
    }
}

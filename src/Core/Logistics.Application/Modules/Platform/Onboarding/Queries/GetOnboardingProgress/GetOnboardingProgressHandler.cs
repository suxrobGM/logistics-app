using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Onboarding.Queries;

internal sealed class GetOnboardingProgressHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetOnboardingProgressQuery, Result<OnboardingProgressDto>>
{
    public async Task<Result<OnboardingProgressDto>> Handle(GetOnboardingProgressQuery req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var operatingMode = tenant.Settings.OperatingMode;

        var steps = new List<OnboardingStepDto>
        {
            new()
            {
                Key = "companyProfile",
                IsComplete = IsAddressComplete(tenant.CompanyAddress)
            },
            new()
            {
                Key = "addTruck",
                IsComplete = await tenantUow.Repository<Truck>().Query().AnyAsync(ct)
            }
        };

        if (operatingMode is not OperatingMode.SoloOperator)
        {
            // A solo operator is the only employee, so "invite your team" can never complete.
            var hasSecondEmployee = await tenantUow.Repository<Employee>()
                .Query().OrderBy(e => e.Id).Skip(1).AnyAsync(ct);
            steps.Add(new OnboardingStepDto { Key = "inviteTeam", IsComplete = hasSecondEmployee });
        }

        steps.Add(new OnboardingStepDto
        {
            Key = "addCustomer",
            IsComplete = await tenantUow.Repository<Customer>().Query().AnyAsync(ct)
        });
        steps.Add(new OnboardingStepDto
        {
            Key = "firstLoad",
            IsComplete = await tenantUow.Repository<Load>().Query().AnyAsync(ct)
        });
        steps.Add(new OnboardingStepDto
        {
            Key = "getPaid",
            IsComplete = tenant.ConnectStatus == StripeConnectStatus.Active
        });
        steps.Add(new OnboardingStepDto
        {
            Key = "connectEld",
            IsComplete = await tenantUow.Repository<EldProviderConfiguration>().Query().AnyAsync(ct)
        });

        return Result<OnboardingProgressDto>.Ok(new OnboardingProgressDto
        {
            OperatingMode = operatingMode,
            Steps = steps
        });
    }

    private static bool IsAddressComplete(Address address)
    {
        return !string.IsNullOrWhiteSpace(address.Line1) &&
               !string.IsNullOrWhiteSpace(address.City) &&
               !string.IsNullOrWhiteSpace(address.State) &&
               !string.IsNullOrWhiteSpace(address.ZipCode) &&
               !string.IsNullOrWhiteSpace(address.Country);
    }
}

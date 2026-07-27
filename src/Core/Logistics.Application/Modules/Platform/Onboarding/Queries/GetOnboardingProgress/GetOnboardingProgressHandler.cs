using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Onboarding.Queries;

internal sealed class GetOnboardingProgressHandler(
    ITenantUnitOfWork tenantUow,
    IFeatureService featureService)
    : IAppRequestHandler<GetOnboardingProgressQuery, Result<OnboardingProgressDto>>
{
    /// <summary>
    /// A step whose feature is switched off can never complete, and the checklist only auto-hides
    /// once every step is done - so an ungated step for a disabled feature pins the card forever.
    /// </summary>
    private sealed record StepDefinition(
        string Key,
        TenantFeature? Feature,
        bool FleetOnly,
        Func<Task<bool>> IsComplete);

    public async Task<Result<OnboardingProgressDto>> Handle(GetOnboardingProgressQuery req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var operatingMode = tenant.Settings.OperatingMode;
        var enabledFeatures = (await featureService.GetEnabledFeaturesAsync(tenant.Id)).ToHashSet();

        StepDefinition[] definitions =
        [
            new("companyProfile", null, false,
                () => Task.FromResult(IsAddressComplete(tenant.CompanyAddress))),
            new("addTruck", TenantFeature.Trucks, false,
                () => tenantUow.Repository<Truck>().Query().AnyAsync(ct)),
            // A solo operator is the only employee, so "invite your team" can never complete.
            new("inviteTeam", TenantFeature.Employees, true,
                () => tenantUow.Repository<Employee>().Query().OrderBy(e => e.Id).Skip(1).AnyAsync(ct)),
            new("addCustomer", TenantFeature.Customers, false,
                () => tenantUow.Repository<Customer>().Query().AnyAsync(ct)),
            new("firstLoad", TenantFeature.Loads, false,
                () => tenantUow.Repository<Load>().Query().AnyAsync(ct)),
            new("getPaid", TenantFeature.Payments, false,
                () => Task.FromResult(tenant.ConnectStatus == StripeConnectStatus.Active)),
            new("connectEld", TenantFeature.Eld, false,
                () => tenantUow.Repository<EldProviderConfiguration>().Query().AnyAsync(ct))
        ];

        var steps = new List<OnboardingStepDto>();
        foreach (var definition in definitions)
        {
            if (definition.FleetOnly && operatingMode is OperatingMode.SoloOperator)
            {
                continue;
            }
            if (definition.Feature is { } feature && !enabledFeatures.Contains(feature))
            {
                continue;
            }

            steps.Add(new OnboardingStepDto
            {
                Key = definition.Key,
                IsComplete = await definition.IsComplete()
            });
        }

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

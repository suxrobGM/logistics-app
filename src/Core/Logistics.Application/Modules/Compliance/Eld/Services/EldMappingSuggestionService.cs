using Logistics.Application.Abstractions.Eld;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Eld.Services;

internal sealed class EldMappingSuggestionService(
    ITenantUnitOfWork tenantUow,
    IEldProviderFactory eldProviderFactory) : IEldMappingSuggestionService
{
    private static readonly string[] DrivingRoles = [TenantRoles.Driver, TenantRoles.Owner];

    public async Task<EldMappingSuggestionsDto> GetSuggestionsAsync(EldProviderConfiguration config, CancellationToken ct)
    {
        var provider = eldProviderFactory.GetProvider(config);

        var driverMappings = await tenantUow.Repository<EldDriverMapping>()
            .GetListAsync(m => m.ProviderType == config.ProviderType, ct);
        var vehicleMappings = await tenantUow.Repository<EldVehicleMapping>()
            .GetListAsync(m => m.ProviderType == config.ProviderType, ct);

        var mappedExternalDrivers = driverMappings.Select(m => m.ExternalDriverId).ToHashSet();
        var mappedEmployees = driverMappings.Select(m => m.EmployeeId).ToList();
        var mappedExternalVehicles = vehicleMappings.Select(m => m.ExternalVehicleId).ToHashSet();
        var mappedTrucks = vehicleMappings.Select(m => m.TruckId).ToList();

        var driversTask = provider.GetAllDriversAsync();
        var vehiclesTask = provider.GetAllVehiclesAsync();
        await Task.WhenAll(driversTask, vehiclesTask);

        var eldDrivers = (await driversTask)
            .Where(d => !string.IsNullOrEmpty(d.ExternalDriverId) && !mappedExternalDrivers.Contains(d.ExternalDriverId))
            .ToList();
        var eldVehicles = (await vehiclesTask)
            .Where(v => !string.IsNullOrEmpty(v.ExternalVehicleId) && !mappedExternalVehicles.Contains(v.ExternalVehicleId))
            .ToList();

        var employees = await tenantUow.Repository<Employee>().Query()
            .Where(e => e.Role != null && DrivingRoles.Contains(e.Role.Name) && !mappedEmployees.Contains(e.Id))
            .Select(e => new
            {
                e.Id,
                e.FirstName,
                e.LastName,
                e.Email,
                Licenses = e.Licenses.Select(l => l.LicenseNumber).ToList()
            })
            .ToListAsync(ct);

        var trucks = await tenantUow.Repository<Truck>().Query()
            .Where(t => !mappedTrucks.Contains(t.Id))
            .Select(t => new TruckCandidate(t.Id, t.Number, t.Vin))
            .ToListAsync(ct);

        var driverCandidates = employees
            .Select(e => new DriverCandidate(e.Id, $"{e.FirstName} {e.LastName}", e.Email, e.Licenses))
            .ToList();

        return new EldMappingSuggestionsDto
        {
            Drivers = EldMappingMatcher.MatchDrivers(eldDrivers, driverCandidates),
            Vehicles = EldMappingMatcher.MatchVehicles(eldVehicles, trucks)
        };
    }
}

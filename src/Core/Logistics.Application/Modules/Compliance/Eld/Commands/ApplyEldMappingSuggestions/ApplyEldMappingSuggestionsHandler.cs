using Logistics.Application.Modules.Compliance.Eld.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

internal sealed class ApplyEldMappingSuggestionsHandler(
    ITenantUnitOfWork tenantUow,
    IEldMappingSuggestionService suggestionService,
    ILogger<ApplyEldMappingSuggestionsHandler> logger)
    : IRequestHandler<ApplyEldMappingSuggestionsCommand, Result<EldMappingSuggestionsDto>>
{
    public async Task<Result<EldMappingSuggestionsDto>> Handle(
        ApplyEldMappingSuggestionsCommand req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<EldProviderConfiguration>().GetByIdAsync(req.ProviderId, ct);
        if (config is null)
        {
            return Result<EldMappingSuggestionsDto>.Fail("ELD provider configuration not found");
        }

        var suggestions = await suggestionService.GetSuggestionsAsync(config, ct);

        await tenantUow.Repository<EldDriverMapping>().AddRangeAsync(suggestions.Drivers.Select(s => new EldDriverMapping
        {
            EmployeeId = s.EmployeeId,
            ProviderType = config.ProviderType,
            ExternalDriverId = s.ExternalDriverId,
            ExternalDriverName = s.ExternalDriverName
        }), ct);

        await tenantUow.Repository<EldVehicleMapping>().AddRangeAsync(suggestions.Vehicles.Select(s => new EldVehicleMapping
        {
            TruckId = s.TruckId,
            ProviderType = config.ProviderType,
            ExternalVehicleId = s.ExternalVehicleId,
            ExternalVehicleName = s.ExternalVehicleName
        }), ct);

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Applied {Drivers} driver and {Vehicles} vehicle ELD mapping suggestions for {ProviderType}",
            suggestions.Drivers.Count, suggestions.Vehicles.Count, config.ProviderType);
        return Result<EldMappingSuggestionsDto>.Ok(suggestions);
    }
}

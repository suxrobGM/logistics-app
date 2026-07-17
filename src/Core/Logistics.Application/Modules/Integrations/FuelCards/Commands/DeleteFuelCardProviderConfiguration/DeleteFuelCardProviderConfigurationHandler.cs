using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

internal sealed class DeleteFuelCardProviderConfigurationHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeleteFuelCardProviderConfigurationCommand, FuelCardProviderConfiguration>(tenantUow);

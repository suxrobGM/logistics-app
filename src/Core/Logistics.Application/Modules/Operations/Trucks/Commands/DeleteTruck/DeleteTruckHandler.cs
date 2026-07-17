using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

internal sealed class DeleteTruckHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeleteTruckCommand, Truck>(tenantUow);

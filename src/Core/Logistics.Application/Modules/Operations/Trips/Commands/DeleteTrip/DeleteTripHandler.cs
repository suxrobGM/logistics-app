using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class DeleteTripHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeleteTripCommand, Trip>(tenantUow);

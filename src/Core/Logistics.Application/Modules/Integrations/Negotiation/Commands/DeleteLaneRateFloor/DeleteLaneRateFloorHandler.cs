using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal sealed class DeleteLaneRateFloorHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeleteLaneRateFloorCommand, LaneRateFloor>(tenantUow);

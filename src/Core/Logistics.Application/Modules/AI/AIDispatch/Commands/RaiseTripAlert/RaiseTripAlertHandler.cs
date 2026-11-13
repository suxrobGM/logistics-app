using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.AIDispatch.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

internal sealed class RaiseTripAlertHandler(
    ITenantUnitOfWork tenantUow,
    IFeatureService featureService,
    ITripAlertTurnStarter turnStarter) : IRequestHandler<RaiseTripAlertCommand, Result>
{
    public async Task<Result> Handle(RaiseTripAlertCommand req, CancellationToken ct)
    {
        await tenantUow.SetCurrentTenantByIdAsync(req.TenantId);

        // A background job: [RequiresFeature] would run before the tenant is set, so check here.
        if (!await featureService.IsFeatureEnabledAsync(req.TenantId, TenantFeature.AITripAlerts))
        {
            return Result.Ok();
        }

        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(req.TripId, ct);
        if (trip is not { IsActive: true })
        {
            return Result.Ok();
        }

        await turnStarter.RaiseAsync(trip, req.Kind, req.Details, ct);
        return Result.Ok();
    }
}

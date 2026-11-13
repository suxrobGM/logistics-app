using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Loads;
using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Portal.Queries;

internal sealed class GetPortalLoadHandler(ITenantUnitOfWork tenantUow, ITrackingLinkService trackingLinks)
    : IRequestHandler<GetPortalLoadQuery, Result<PortalLoadDto>>
{
    public async Task<Result<PortalLoadDto>> Handle(
        GetPortalLoadQuery req,
        CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().Query()
            .Where(l => l.Id == req.LoadId && l.CustomerId == req.CustomerId)
            .FirstOrDefaultAsync(ct);

        if (load == null)
        {
            return Result<PortalLoadDto>.Fail("Load not found or access denied.", ErrorCodes.NotFound);
        }

        var nextStop = load.TripStops
            .Where(s => s.ArrivedAt == null)
            .OrderBy(s => s.Position)
            .FirstOrDefault();
        var trackingLink = await trackingLinks.FindActiveAsync(load.Id, ct);

        var dto = new PortalLoadDto
        {
            Id = load.Id,
            Number = load.Number,
            Name = load.Name,
            Status = load.Status,
            Type = load.Type,
            Vehicles = load.Vehicles.ToDto(),
            OriginAddress = load.OriginAddress,
            DestinationAddress = load.DestinationAddress,
            CurrentAddress = load.AssignedTruck?.CurrentAddress,
            CurrentLocation = load.AssignedTruck?.CurrentLocation,
            DispatchedAt = load.DispatchedAt,
            PickedUpAt = load.PickedUpAt,
            DeliveredAt = load.DeliveredAt,
            CreatedAt = load.CreatedAt,
            DeliveryCost = load.DeliveryCost,
            Distance = load.Distance,
            DriverName = load.AssignedTruck != null ? string.Join(", ", load.AssignedTruck.GetDriversNames()) : null,
            TruckNumber = load.AssignedTruck?.Number,
            DocumentCount = load.Documents.Count,
            HasProofOfDelivery = load.Documents.Any(d => d.Type == DocumentType.ProofOfDelivery),
            HasBillOfLading = load.Documents.Any(d => d.Type == DocumentType.BillOfLading),
            PickupAppointmentFrom = load.PickupAppointmentFrom,
            PickupAppointmentTo = load.PickupAppointmentTo,
            DeliveryAppointmentFrom = load.DeliveryAppointmentFrom,
            DeliveryAppointmentTo = load.DeliveryAppointmentTo,
            EstimatedArrivalAt = nextStop?.EstimatedArrivalAt,
            IsRunningLate = nextStop?.LateNotifiedAt is not null,
            TrackingUrl = trackingLink is null ? null : trackingLinks.GetUrl(trackingLink),
            Updates =
            [
                .. load.CustomerUpdates
                    .Where(u => u.Outcome == CustomerUpdateOutcome.Sent)
                    .Select(u => new PortalShipmentUpdateDto { Event = u.Event, At = u.At })
            ]
        };

        return Result<PortalLoadDto>.Ok(dto);
    }
}

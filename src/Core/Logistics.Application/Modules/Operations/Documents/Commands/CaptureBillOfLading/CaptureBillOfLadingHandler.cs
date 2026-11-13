using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Documents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Documents.Commands;

internal sealed class CaptureBillOfLadingHandler(IDeliveryDocumentService deliveryDocuments)
    : IRequestHandler<CaptureBillOfLadingCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CaptureBillOfLadingCommand req, CancellationToken ct)
    {
        return deliveryDocuments.CaptureAsync(
            DeliveryDocumentKind.BillOfLading,
            new CaptureDeliveryDocumentParameters(
                req.LoadId,
                req.TripStopId,
                req.Photos,
                req.SignatureBase64,
                req.RecipientName,
                req.Latitude,
                req.Longitude,
                req.Notes,
                req.UnableToSignReason),
            ct);
    }
}

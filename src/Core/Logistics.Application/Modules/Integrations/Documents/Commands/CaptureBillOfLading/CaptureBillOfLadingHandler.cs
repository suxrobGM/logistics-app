using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Documents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Commands;

internal sealed class CaptureBillOfLadingHandler(IDeliveryDocumentService deliveryDocuments)
    : IAppRequestHandler<CaptureBillOfLadingCommand, Result<Guid>>
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
                req.Notes),
            ct);
    }
}

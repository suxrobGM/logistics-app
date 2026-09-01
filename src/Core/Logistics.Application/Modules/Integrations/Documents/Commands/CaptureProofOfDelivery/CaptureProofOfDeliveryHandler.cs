using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Documents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Commands;

internal sealed class CaptureProofOfDeliveryHandler(IDeliveryDocumentService deliveryDocuments)
    : IAppRequestHandler<CaptureProofOfDeliveryCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CaptureProofOfDeliveryCommand req, CancellationToken ct)
    {
        return deliveryDocuments.CaptureAsync(
            DeliveryDocumentKind.ProofOfDelivery,
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

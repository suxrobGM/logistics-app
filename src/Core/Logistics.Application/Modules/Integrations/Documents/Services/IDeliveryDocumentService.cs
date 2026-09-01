using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Services;

/// <summary>
///     Stores the paperwork a driver captures when picking up or delivering a load.
/// </summary>
public interface IDeliveryDocumentService : IApplicationService
{
    /// <summary>
    ///     Uploads the signature and photos, then records one delivery document per photo.
    ///     Uploaded files are removed again when nothing is saved.
    /// </summary>
    /// <param name="kind">Whether this is a bill of lading or a proof of delivery.</param>
    /// <param name="parameters">What the driver captured.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The ID of the first document created, or a failure when the caller cannot reach the load.</returns>
    Task<Result<Guid>> CaptureAsync(
        DeliveryDocumentKind kind,
        CaptureDeliveryDocumentParameters parameters,
        CancellationToken ct = default);
}

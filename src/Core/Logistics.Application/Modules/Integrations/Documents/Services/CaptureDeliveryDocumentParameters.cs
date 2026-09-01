using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Services;

/// <summary>
///     What the driver captured at a stop.
/// </summary>
/// <param name="LoadId">The load the paperwork belongs to.</param>
/// <param name="TripStopId">The stop it was captured at, when the load is part of a trip.</param>
/// <param name="Photos">Photographed pages. May be empty when only a signature was collected.</param>
/// <param name="SignatureBase64">The recipient's signature as a base64 PNG.</param>
/// <param name="RecipientName">Who signed for the load.</param>
/// <param name="Latitude">Where the capture happened.</param>
/// <param name="Longitude">Where the capture happened.</param>
/// <param name="Notes">Free-form notes from the driver.</param>
public sealed record CaptureDeliveryDocumentParameters(
    Guid LoadId,
    Guid? TripStopId,
    List<FileUpload> Photos,
    string? SignatureBase64,
    string? RecipientName,
    double? Latitude,
    double? Longitude,
    string? Notes);

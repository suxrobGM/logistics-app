using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class CaptureProofOfDeliveryCommand : IAppRequest<Result<Guid>>
{
    public required Guid LoadId { get; set; }
    public Guid? TripStopId { get; set; }

    // Photos (multiple allowed)
    public required List<FileUpload> Photos { get; set; } = [];

    // Signature
    public string? SignatureBase64 { get; set; }

    // Recipient info
    public string? RecipientName { get; set; }

    // Location
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Additional notes
    public string? Notes { get; set; }

    // Who is capturing this POD
    public required Guid CapturedById { get; set; }
}

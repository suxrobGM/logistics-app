using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Abstractions.Documents;

/// <summary>
/// Renders the car hauler electronic bill of lading: the load's vehicles, each one's damage at
/// pickup and delivery on the vehicle diagram, and the shipper and receiver signatures.
/// </summary>
public interface IVehicleBolPdfService
{
    byte[] Generate(VehicleBolDocument document);
}

/// <summary>
/// Everything the eBOL prints, gathered by the application layer so the renderer touches no
/// database or blob storage.
/// </summary>
public sealed record VehicleBolDocument(
    Tenant Carrier,
    long LoadNumber,
    string LoadName,
    string CustomerName,
    Address Origin,
    Address Destination,
    string? TruckNumber,
    string? DriverName,
    string Currency,
    IReadOnlyList<VehicleBolVehicle> Vehicles,
    VehicleBolSignature? Pickup,
    VehicleBolSignature? Delivery,
    DateTime GeneratedAt);

public sealed record VehicleBolVehicle(
    LoadVehicle Vehicle,
    VehicleBolInspection? Pickup,
    VehicleBolInspection? Delivery);

public sealed record VehicleBolInspection(
    DateTime InspectedAt,
    string? InspectorName,
    double? Latitude,
    double? Longitude,
    string? Notes,
    IReadOnlyList<ConditionDefect> Defects);

/// <summary>
/// The shipper (pickup) or receiver (delivery) sign-off. <see cref="SignatureImage"/> is the PNG the
/// driver captured; null with <see cref="UnableToSignReason"/> set when nobody signed.
/// </summary>
public sealed record VehicleBolSignature(
    string? SignerName,
    byte[]? SignatureImage,
    string? UnableToSignReason,
    DateTime? SignedAt,
    double? Latitude,
    double? Longitude);

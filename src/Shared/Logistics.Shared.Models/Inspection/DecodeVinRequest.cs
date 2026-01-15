namespace Logistics.Shared.Models.Inspection;

/// <summary>
/// Request to decode a VIN using the NHTSA API.
/// </summary>
public record DecodeVinRequest(string Vin);

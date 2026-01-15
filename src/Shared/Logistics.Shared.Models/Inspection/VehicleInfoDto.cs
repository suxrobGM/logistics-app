namespace Logistics.Shared.Models;

public record VehicleInfoDto(
    string Vin,
    int? Year,
    string? Make,
    string? Model,
    string? BodyClass,
    string? VehicleType,
    string? DriveType,
    string? FuelType,
    string? EngineInfo);

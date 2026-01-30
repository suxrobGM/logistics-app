using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class AccidentMapper
{
    public static AccidentReportDto ToDto(this AccidentReport entity)
    {
        return new AccidentReportDto
        {
            Id = entity.Id,
            TruckId = entity.TruckId,
            TruckNumber = entity.Truck?.Number ?? string.Empty,
            DriverId = entity.DriverId,
            DriverName = entity.Driver?.GetFullName() ?? string.Empty,
            Type = entity.AccidentType,
            TypeDisplay = entity.AccidentType.GetDescription(),
            Severity = entity.Severity,
            SeverityDisplay = entity.Severity.GetDescription(),
            Status = entity.Status,
            StatusDisplay = entity.Status.GetDescription(),
            AccidentDateTime = entity.AccidentDateTime,
            Location = entity.Address ?? string.Empty,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Description = entity.Description,
            WeatherConditions = entity.WeatherConditions,
            RoadConditions = entity.RoadConditions,
            PoliceReportFiled = entity.PoliceReportFiled,
            PoliceReportNumber = entity.PoliceReportNumber,
            PoliceOfficerName = entity.PoliceOfficerName,
            PoliceDepartment = entity.PoliceDepartment,
            InjuriesReported = entity.AnyInjuries,
            NumberOfInjuries = entity.NumberOfInjuries,
            InjuryDescription = entity.InjuryDescription,
            EstimatedDamage = entity.EstimatedDamageCost,
            DamageDescription = entity.VehicleDamageDescription,
            InsuranceClaimNumber = entity.InsuranceClaimNumber,
            InsuranceNotifiedAt = entity.InsuranceNotifiedAt,
            ReviewedById = entity.ReviewedById,
            ReviewedByName = entity.ReviewedBy?.GetFullName(),
            ReviewedAt = entity.ReviewedAt,
            ReviewNotes = entity.ReviewNotes,
            TripId = entity.TripId,
            Witnesses = entity.Witnesses.Select(w => w.ToDto()).ToList(),
            ThirdParties = entity.ThirdParties.Select(tp => tp.ToDto()).ToList(),
            Documents = entity.Documents.Select(d => d.ToDto()).ToList(),
            CreatedAt = entity.CreatedAt
        };
    }

    public static AccidentWitnessDto ToDto(this AccidentWitness entity)
    {
        return new AccidentWitnessDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Phone = entity.PhoneNumber,
            Email = entity.Email,
            Address = entity.Address,
            Statement = entity.Statement
        };
    }

    public static AccidentThirdPartyDto ToDto(this AccidentThirdParty entity)
    {
        return new AccidentThirdPartyDto
        {
            Id = entity.Id,
            DriverName = entity.Name,
            DriverPhone = entity.PhoneNumber,
            DriverLicense = entity.DriverLicense,
            VehicleMake = entity.VehicleMake,
            VehicleModel = entity.VehicleModel,
            VehicleYear = entity.VehicleYear?.ToString(),
            VehicleColor = entity.VehicleColor,
            LicensePlate = entity.VehicleLicensePlate,
            VinNumber = entity.VehicleVin,
            InsuranceCompany = entity.InsuranceCompany,
            InsurancePolicyNumber = entity.InsurancePolicyNumber,
            InsurancePhone = entity.InsuranceAgentPhone
        };
    }
}

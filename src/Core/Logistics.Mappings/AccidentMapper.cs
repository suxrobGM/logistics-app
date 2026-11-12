using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class AccidentMapper
{
    extension(AccidentReport entity)
    {
        /// <summary>For one row: reads the related names off the navigations. Lists pass them in
        /// through the overload instead, looked up for the whole page at once.</summary>
        public AccidentReportDto ToDto() => entity.ToDto(
            entity.Truck?.Number,
            entity.Driver?.GetFullName(),
            entity.ReviewedBy?.GetFullName(),
            entity.Witnesses.Select(w => w.ToDto()),
            entity.ThirdParties.Select(tp => tp.ToDto()));

        public AccidentReportDto ToDto(
            string? truckNumber,
            string? driverName,
            string? reviewerName,
            IEnumerable<AccidentWitnessDto> witnesses,
            IEnumerable<AccidentThirdPartyDto> thirdParties)
        {
            return new AccidentReportDto
            {
                Id = entity.Id,
                TruckId = entity.TruckId,
                TruckNumber = truckNumber ?? string.Empty,
                DriverId = entity.DriverId,
                DriverName = driverName ?? string.Empty,
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
                Description = entity.Description ?? string.Empty,
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
                ReviewedByName = reviewerName,
                ReviewedAt = entity.ReviewedAt,
                ReviewNotes = entity.ReviewNotes,
                TripId = entity.TripId,
                Witnesses = witnesses.ToList(),
                ThirdParties = thirdParties.ToList(),
                CreatedAt = entity.CreatedAt
            };
        }
    }

    extension(AccidentWitness entity)
    {
        public AccidentWitnessDto ToDto()
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
    }

    extension(AccidentThirdParty entity)
    {
        public AccidentThirdPartyDto ToDto()
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
}

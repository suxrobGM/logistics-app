using Logistics.Application.Common.Mapping;
using Logistics.Domain.Entities.Safety;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Compliance.Safety;

[Mapper]
public static partial class AccidentMapper
{
    [MapperIgnoreSource(nameof(AccidentReport.Truck))]
    [MapperIgnoreSource(nameof(AccidentReport.Driver))]
    [MapperIgnoreSource(nameof(AccidentReport.ReviewedBy))]
    [MapperIgnoreSource(nameof(AccidentReport.Trip))]
    [MapperIgnoreSource(nameof(AccidentReport.Witnesses))]
    [MapperIgnoreSource(nameof(AccidentReport.ThirdParties))]
    [MapperIgnoreTarget(nameof(AccidentReportDto.TruckNumber))]
    [MapperIgnoreTarget(nameof(AccidentReportDto.DriverName))]
    [MapperIgnoreTarget(nameof(AccidentReportDto.ReviewedByName))]
    [MapperIgnoreTarget(nameof(AccidentReportDto.Witnesses))]
    [MapperIgnoreTarget(nameof(AccidentReportDto.ThirdParties))]
    [MapProperty(nameof(AccidentReport.AccidentType), nameof(AccidentReportDto.Type))]
    [MapProperty(nameof(AccidentReport.AccidentType), nameof(AccidentReportDto.TypeDisplay), Use = nameof(@MappingConversions.Describe))]
    [MapProperty(nameof(AccidentReport.Severity), nameof(AccidentReportDto.SeverityDisplay), Use = nameof(@MappingConversions.Describe))]
    [MapProperty(nameof(AccidentReport.Status), nameof(AccidentReportDto.StatusDisplay), Use = nameof(@MappingConversions.Describe))]
    [MapProperty(nameof(AccidentReport.Address), nameof(AccidentReportDto.Location), Use = nameof(@MappingConversions.DefaultToEmpty))]
    [MapProperty(nameof(AccidentReport.Description), nameof(AccidentReportDto.Description), Use = nameof(@MappingConversions.DefaultToEmpty))]
    [MapProperty(nameof(AccidentReport.AnyInjuries), nameof(AccidentReportDto.InjuriesReported))]
    [MapProperty(nameof(AccidentReport.EstimatedDamageCost), nameof(AccidentReportDto.EstimatedDamage))]
    [MapProperty(nameof(AccidentReport.VehicleDamageDescription), nameof(AccidentReportDto.DamageDescription))]
    private static partial AccidentReportDto MapAccidentReport(AccidentReport entity);

    [MapperIgnoreSource(nameof(AccidentWitness.AccidentReport))]
    [MapProperty(nameof(AccidentWitness.PhoneNumber), nameof(AccidentWitnessDto.Phone))]
    public static partial AccidentWitnessDto ToDto(this AccidentWitness entity);

    [MapperIgnoreSource(nameof(AccidentThirdParty.AccidentReport))]
    [MapProperty(nameof(AccidentThirdParty.Name), nameof(AccidentThirdPartyDto.DriverName))]
    [MapProperty(nameof(AccidentThirdParty.PhoneNumber), nameof(AccidentThirdPartyDto.DriverPhone))]
    [MapProperty(nameof(AccidentThirdParty.VehicleLicensePlate), nameof(AccidentThirdPartyDto.LicensePlate))]
    [MapProperty(nameof(AccidentThirdParty.VehicleVin), nameof(AccidentThirdPartyDto.VinNumber))]
    [MapProperty(nameof(AccidentThirdParty.InsuranceAgentPhone), nameof(AccidentThirdPartyDto.InsurancePhone))]
    public static partial AccidentThirdPartyDto ToDto(this AccidentThirdParty entity);

    extension(AccidentReport entity)
    {
        /// <summary>Single rows only; lists batch the names and use the overload.</summary>
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
            var dto = MapAccidentReport(entity);
            dto.TruckNumber = truckNumber ?? string.Empty;
            dto.DriverName = driverName ?? string.Empty;
            dto.ReviewedByName = reviewerName;
            dto.Witnesses = witnesses.ToList();
            dto.ThirdParties = thirdParties.ToList();
            return dto;
        }
    }
}

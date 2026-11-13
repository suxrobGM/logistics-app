using Logistics.Application.Modules.Operations.Loads;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Operations.Trips;

[Mapper]
public static partial class TripMapper
{
    [MapperIgnoreSource(nameof(Trip.Truck))]
    [MapperIgnoreSource(nameof(Trip.Trailer))]
    [MapperIgnoreSource(nameof(Trip.Stops))]
    [MapperIgnoreTarget(nameof(TripDto.TruckNumber))]
    [MapperIgnoreTarget(nameof(TripDto.TrailerNumber))]
    [MapperIgnoreTarget(nameof(TripDto.Stops))]
    [MapperIgnoreTarget(nameof(TripDto.Loads))]
    [MapperIgnoreTarget(nameof(TripDto.LoadsCount))]
    [MapperIgnoreTarget(nameof(TripDto.TotalRevenue))]
    [MapProperty(nameof(Trip.CreatedAt), nameof(TripDto.CreatedAt))]
    private static partial TripDto MapTrip(Trip entity, Address originAddress, Address destinationAddress);

    extension(Trip entity)
    {
        /// <summary>Single rows only; lists batch the truck, stops and loads and pass them in.</summary>
        public TripDto ToDto(string? trailerNumber) => entity.ToDto(
            entity.Truck?.Number,
            trailerNumber,
            entity.Stops,
            [.. entity.GetLoads().Select(LoadMapper.ToTripLoadDto)]);

        /// <param name="loads">The distinct loads on <paramref name="stops"/>.</param>
        public TripDto ToDto(
            string? truckNumber,
            string? trailerNumber,
            IReadOnlyCollection<TripStop> stops,
            IReadOnlyList<TripLoadDto> loads)
        {
            var ordered = stops.OrderBy(s => s.Position).ToArray();
            var deliveryCosts = loads.ToDictionary(l => l.Id, l => l.DeliveryCost);

            var dto = MapTrip(entity, ordered.First().Address, ordered.Last().Address);
            dto.TruckNumber = truckNumber;
            dto.TrailerNumber = trailerNumber;
            dto.Stops = stops.Select(TripStopMapper.ToDto).ToArray();
            dto.Loads = loads;
            dto.LoadsCount = loads.Count;
            dto.TotalRevenue = stops
                .Where(s => s.Type == TripStopType.DropOff)
                .Sum(s => deliveryCosts[s.LoadId]);
            return dto;
        }
    }
}

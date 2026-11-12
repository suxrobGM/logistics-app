using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
[UseStaticMapper(typeof(CustomerMapper))]
[UseStaticMapper(typeof(LoadVehicleMapper))]
[UseStaticMapper(typeof(CustomerUpdateMapper))]
public static partial class LoadMapper
{
    [MapperIgnoreSource(nameof(Load.Customer))]
    [MapperIgnoreSource(nameof(Load.AssignedTruck))]
    [MapperIgnoreSource(nameof(Load.AssignedDispatcher))]
    [MapperIgnoreSource(nameof(Load.Invoice))]
    [MapperIgnoreSource(nameof(Load.Documents))]
    [MapperIgnoreSource(nameof(Load.Exceptions))]
    [MapperIgnoreSource(nameof(Load.TripStops))]
    [MapperIgnoreSource(nameof(Load.Container))]
    [MapperIgnoreSource(nameof(Load.OriginTerminal))]
    [MapperIgnoreSource(nameof(Load.DestinationTerminal))]
    [MapperIgnoreSource(nameof(Load.Accessorials))]
    [MapperIgnoreTarget(nameof(LoadDto.Customer))]
    [MapperIgnoreTarget(nameof(LoadDto.Invoice))]
    [MapperIgnoreTarget(nameof(LoadDto.Accessorials))]
    [MapperIgnoreTarget(nameof(LoadDto.ContainerNumber))]
    [MapperIgnoreTarget(nameof(LoadDto.ContainerIsoType))]
    [MapperIgnoreTarget(nameof(LoadDto.ContainerLastFreeDay))]
    [MapperIgnoreTarget(nameof(LoadDto.ContainerIsOnHold))]
    [MapperIgnoreTarget(nameof(LoadDto.OriginTerminalName))]
    [MapperIgnoreTarget(nameof(LoadDto.OriginTerminalCode))]
    [MapperIgnoreTarget(nameof(LoadDto.DestinationTerminalName))]
    [MapperIgnoreTarget(nameof(LoadDto.DestinationTerminalCode))]
    [MapperIgnoreTarget(nameof(LoadDto.AssignedDispatcherName))]
    [MapperIgnoreTarget(nameof(LoadDto.AssignedTruckNumber))]
    [MapperIgnoreTarget(nameof(LoadDto.AssignedTruckDriversName))]
    [MapperIgnoreTarget(nameof(LoadDto.CurrentAddress))]
    [MapperIgnoreTarget(nameof(LoadDto.CurrentLocation))]
    [MapperIgnoreTarget(nameof(LoadDto.TripId))]
    [MapperIgnoreTarget(nameof(LoadDto.TripName))]
    [MapperIgnoreTarget(nameof(LoadDto.TripNumber))]
    [MapperIgnoreTarget(nameof(LoadDto.TrailerId))]
    [MapperIgnoreTarget(nameof(LoadDto.TrailerNumber))]
    [MapperIgnoreTarget(nameof(LoadDto.VehicleCapacityWarning))]
    private static partial LoadDto MapLoad(Load entity);

    extension(Load entity)
    {
        /// <summary>
        /// Related values come from <paramref name="lookup"/> only - there is no navigation-property
        /// fallback, so a handler that skips the resolver gets blank fields instead of silently paying
        /// a lazy SELECT per row. Single-row callers resolve a one-load batch.
        /// </summary>
        public LoadDto ToDto(LoadLookup lookup)
        {
            var container = lookup.FindContainer(entity.ContainerId);
            var originTerminal = lookup.FindTerminal(entity.OriginTerminalId);
            var destinationTerminal = lookup.FindTerminal(entity.DestinationTerminalId);
            var truck = lookup.FindTruck(entity.AssignedTruckId);
            var trip = lookup.FindTrip(entity.Id);

            var dto = MapLoad(entity);
            dto.ContainerNumber = container?.Number;
            dto.ContainerIsoType = container?.IsoType;
            dto.ContainerLastFreeDay = container?.LastFreeDay;
            dto.ContainerIsOnHold = container?.IsOnHold;
            dto.OriginTerminalName = originTerminal?.Name;
            dto.OriginTerminalCode = originTerminal?.Code;
            dto.DestinationTerminalName = destinationTerminal?.Name;
            dto.DestinationTerminalCode = destinationTerminal?.Code;
            dto.AssignedDispatcherName = lookup.FindEmployeeName(entity.AssignedDispatcherId);
            dto.AssignedTruckNumber = truck?.Number;
            dto.AssignedTruckDriversName = truck?.DriversNames;
            dto.Customer = lookup.FindCustomer(entity.CustomerId);
            dto.Invoice = lookup.FindInvoice(entity.Id);
            dto.TripId = trip?.Id;
            dto.TripName = trip?.Name;
            dto.TripNumber = trip?.Number;
            dto.CurrentAddress = truck?.CurrentAddress;
            dto.CurrentLocation = truck?.CurrentLocation;
            return dto;
        }
    }

    [MapperIgnoreSource(nameof(Load.Invoice))]
    [MapperIgnoreSource(nameof(Load.AssignedDispatcher))]
    [MapperIgnoreSource(nameof(Load.AssignedTruck))]
    [MapperIgnoreSource(nameof(Load.TripStops))]
    [MapProperty(nameof(Load.OriginLocation), nameof(TripLoadDto.OriginLocation))]
    [MapProperty(nameof(Load.DestinationLocation), nameof(TripLoadDto.DestinationLocation))]
    [MapProperty(nameof(Load.Type), nameof(TripLoadDto.Type))]
    public static partial TripLoadDto ToTripLoadDto(this Load load);
}

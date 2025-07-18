﻿using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class LoadMapper
{
    public static LoadDto ToDto(this Load entity)
    {
        var dto = new LoadDto
        {
            Id = entity.Id,
            Number = entity.Number,
            Name = entity.Name,
            Type = entity.Type,
            OriginAddress = entity.OriginAddress.ToDto(),
            OriginAddressLat = entity.OriginAddressLat,
            OriginAddressLong = entity.OriginAddressLong,
            DestinationAddress = entity.DestinationAddress.ToDto(),
            DestinationAddressLat = entity.DestinationAddressLat,
            DestinationAddressLong = entity.DestinationAddressLong,
            DispatchedDate = entity.DispatchedDate,
            PickUpDate = entity.PickUpDate,
            DeliveryDate = entity.DeliveryDate,
            DeliveryCost = entity.DeliveryCost,
            Distance = entity.Distance,
            CanConfirmPickUp = entity.CanConfirmPickUp,
            CanConfirmDelivery = entity.CanConfirmDelivery,
            Status = entity.Status,
            AssignedDispatcherId = entity.AssignedDispatcherId,
            AssignedDispatcherName = entity.AssignedDispatcher?.GetFullName(),
            AssignedTruckId = entity.AssignedTruckId,
            AssignedTruckNumber = entity.AssignedTruck?.Number,
            AssignedTruckDriversName = entity.AssignedTruck?.Drivers.Select(i => i.GetFullName()),
            Customer = entity.Customer?.ToDto(),
            Invoices = entity.Invoices.Select(i => i.ToDto()),
        };
        
        if (entity.AssignedTruck?.CurrentLocation.IsNotNull() ?? false)
        {
            dto.CurrentLocation = entity.AssignedTruck.CurrentLocation.ToDto();
        }
        return dto;
    }
}

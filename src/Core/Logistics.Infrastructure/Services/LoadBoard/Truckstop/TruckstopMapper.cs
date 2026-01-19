using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Services.Truckstop;

internal static class TruckstopMapper
{
    public static LoadBoardListingDto ToListingDto(TruckstopLoad load)
    {
        return new LoadBoardListingDto
        {
            ExternalListingId = load.LoadId ?? string.Empty,
            ProviderType = LoadBoardProviderType.Truckstop,
            ProviderName = "Truckstop",
            OriginAddress = new Address
            {
                Line1 = "N/A",
                City = load.Origin?.City ?? "Unknown",
                State = load.Origin?.StateProvince ?? "Unknown",
                ZipCode = load.Origin?.PostalCode ?? "00000",
                Country = "US"
            },
            OriginLocation = new GeoPoint(load.Origin?.Longitude ?? 0, load.Origin?.Latitude ?? 0),
            DestinationAddress = new Address
            {
                Line1 = "N/A",
                City = load.Destination?.City ?? "Unknown",
                State = load.Destination?.StateProvince ?? "Unknown",
                ZipCode = load.Destination?.PostalCode ?? "00000",
                Country = "US"
            },
            DestinationLocation = new GeoPoint(load.Destination?.Longitude ?? 0, load.Destination?.Latitude ?? 0),
            RatePerMile = load.RatePerMile,
            TotalRate = load.Rate,
            Currency = "USD",
            Distance = load.Miles,
            Weight = load.Weight,
            Length = load.Length,
            PickupDateStart = load.PickupDate,
            PickupDateEnd = load.PickupDateEnd,
            DeliveryDateStart = load.DeliveryDate,
            DeliveryDateEnd = load.DeliveryDateEnd,
            EquipmentType = load.EquipmentType,
            Commodity = load.Commodity,
            BrokerName = load.Broker?.CompanyName,
            BrokerPhone = load.Broker?.Phone,
            BrokerEmail = load.Broker?.Email,
            BrokerMcNumber = load.Broker?.McNumber,
            Status = LoadBoardListingStatus.Available,
            ExpiresAt = load.ExpiresAt ?? DateTime.MaxValue
        };
    }

    public static PostedTruckDto ToPostedTruckDto(TruckstopTruck truck)
    {
        return new PostedTruckDto
        {
            ExternalPostId = truck.TruckId,
            ProviderType = LoadBoardProviderType.Truckstop,
            ProviderName = "Truckstop",
            AvailableAtAddress = new Address
            {
                Line1 = "N/A",
                City = truck.Origin?.City ?? "Unknown",
                State = truck.Origin?.StateProvince ?? "Unknown",
                ZipCode = truck.Origin?.PostalCode ?? "00000",
                Country = "US"
            },
            AvailableAtLocation = new GeoPoint(truck.Origin?.Longitude ?? 0, truck.Origin?.Latitude ?? 0),
            EquipmentType = truck.EquipmentType,
            AvailableFrom = truck.AvailableDate ?? DateTime.UtcNow,
            AvailableTo = truck.AvailableDateEnd,
            ExpiresAt = truck.ExpiresAt
        };
    }

    public static LoadBoardWebhookEventType MapWebhookEventType(string? eventType)
    {
        return eventType?.ToLowerInvariant() switch
        {
            "load_posted" => LoadBoardWebhookEventType.LoadPosted,
            "load_cancelled" => LoadBoardWebhookEventType.LoadCancelled,
            "load_expired" => LoadBoardWebhookEventType.LoadExpired,
            "rate_updated" or "rate_changed" => LoadBoardWebhookEventType.RateUpdated,
            "status_changed" => LoadBoardWebhookEventType.StatusChanged,
            "truck_expired" => LoadBoardWebhookEventType.TruckPostExpired,
            _ => LoadBoardWebhookEventType.Unknown
        };
    }
}

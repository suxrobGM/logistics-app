using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.OneTwo3;

internal static class OneTwo3Mapper
{
    public static LoadBoardListingDto ToListingDto(OneTwo3Load load)
    {
        return new LoadBoardListingDto
        {
            ExternalListingId = load.Id ?? string.Empty,
            ProviderType = LoadBoardProviderType.OneTwo3Loadboard,
            ProviderName = "123Loadboard",
            OriginAddress =
                new Address
                {
                    Line1 = "N/A",
                    City = load.Origin?.City ?? "Unknown",
                    State = load.Origin?.State ?? "Unknown",
                    ZipCode = load.Origin?.ZipCode ?? "00000",
                    Country = "US"
                },
            OriginLocation = new GeoPoint(load.Origin?.Lng ?? 0, load.Origin?.Lat ?? 0),
            DestinationAddress =
                new Address
                {
                    Line1 = "N/A",
                    City = load.Destination?.City ?? "Unknown",
                    State = load.Destination?.State ?? "Unknown",
                    ZipCode = load.Destination?.ZipCode ?? "00000",
                    Country = "US"
                },
            DestinationLocation = new GeoPoint(load.Destination?.Lng ?? 0, load.Destination?.Lat ?? 0),
            RatePerMile = load.RatePerMile,
            TotalRate = load.TotalRate,
            Currency = "USD",
            Distance = load.Miles,
            Weight = load.Weight,
            Length = load.Length,
            PickupDateStart = load.PickupStart,
            PickupDateEnd = load.PickupEnd,
            DeliveryDateStart = load.DeliveryStart,
            DeliveryDateEnd = load.DeliveryEnd,
            EquipmentType = load.Equipment,
            Commodity = load.Commodity,
            BrokerName = load.Poster?.CompanyName,
            BrokerPhone = load.Poster?.Phone,
            BrokerEmail = load.Poster?.Email,
            BrokerMcNumber = load.Poster?.McNumber,
            Status = LoadBoardListingStatus.Available,
            ExpiresAt = load.ExpiresAt ?? DateTime.MaxValue
        };
    }

    public static PostedTruckDto ToPostedTruckDto(OneTwo3Truck truck)
    {
        return new PostedTruckDto
        {
            ExternalPostId = truck.Id,
            ProviderType = LoadBoardProviderType.OneTwo3Loadboard,
            ProviderName = "123Loadboard",
            AvailableAtAddress =
                new Address
                {
                    Line1 = "N/A",
                    City = truck.Origin?.City ?? "Unknown",
                    State = truck.Origin?.State ?? "Unknown",
                    ZipCode = truck.Origin?.ZipCode ?? "00000",
                    Country = "US"
                },
            AvailableAtLocation = new GeoPoint(truck.Origin?.Lng ?? 0, truck.Origin?.Lat ?? 0),
            EquipmentType = truck.Equipment,
            AvailableFrom = truck.AvailableFrom ?? DateTime.UtcNow,
            AvailableTo = truck.AvailableTo,
            ExpiresAt = truck.ExpiresAt
        };
    }

    public static LoadBoardWebhookEventType MapWebhookEventType(string? eventType)
    {
        return eventType?.ToLowerInvariant() switch
        {
            "load.posted" => LoadBoardWebhookEventType.LoadPosted,
            "load.removed" or "load.cancelled" => LoadBoardWebhookEventType.LoadCancelled,
            "load.expired" => LoadBoardWebhookEventType.LoadExpired,
            "rate.updated" => LoadBoardWebhookEventType.RateUpdated,
            "status.changed" => LoadBoardWebhookEventType.StatusChanged,
            "truck.expired" => LoadBoardWebhookEventType.TruckPostExpired,
            _ => LoadBoardWebhookEventType.Unknown
        };
    }
}

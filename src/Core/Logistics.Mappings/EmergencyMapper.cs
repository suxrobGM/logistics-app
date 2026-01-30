using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class EmergencyMapper
{
    public static EmergencyAlertDto ToDto(this EmergencyAlert entity)
    {
        return new EmergencyAlertDto
        {
            Id = entity.Id,
            DriverId = entity.DriverId,
            DriverName = entity.Driver?.GetFullName() ?? string.Empty,
            TruckId = entity.TruckId,
            TruckNumber = entity.Truck?.Number,
            TripId = entity.TripId,
            Type = entity.AlertType,
            TypeDisplay = entity.AlertType.GetDescription(),
            Status = entity.Status,
            StatusDisplay = entity.Status.GetDescription(),
            Source = entity.Source,
            SourceDisplay = entity.Source.GetDescription(),
            TriggeredAt = entity.TriggeredAt,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Address = entity.Address,
            DriverMessage = entity.Description,
            AcknowledgedById = entity.AcknowledgedById,
            AcknowledgedByName = entity.AcknowledgedBy?.GetFullName(),
            AcknowledgedAt = entity.AcknowledgedAt,
            ResolvedById = entity.ResolvedById,
            ResolvedByName = entity.ResolvedBy?.GetFullName(),
            ResolvedAt = entity.ResolvedAt,
            ResolutionNotes = entity.ResolutionNotes,
            Notifications = entity.Notifications.Select(n => n.ToDto()).ToList(),
            CreatedAt = entity.CreatedAt
        };
    }

    public static EmergencyContactDto ToDto(this EmergencyContact entity)
    {
        return new EmergencyContactDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeName = entity.Employee?.GetFullName(),
            Name = entity.Name,
            Type = entity.ContactType,
            TypeDisplay = entity.ContactType.GetDescription(),
            Phone = entity.PhoneNumber,
            Email = entity.Email,
            Priority = entity.Priority,
            IsActive = entity.IsActive
        };
    }

    public static EmergencyContactNotificationDto ToDto(this EmergencyContactNotification entity)
    {
        return new EmergencyContactNotificationDto
        {
            Id = entity.Id,
            ContactId = entity.EmergencyContactId,
            ContactName = entity.EmergencyContact?.Name ?? string.Empty,
            ContactPhone = entity.EmergencyContact?.PhoneNumber ?? string.Empty,
            Method = entity.Method,
            SentAt = entity.SentAt,
            Delivered = entity.IsDelivered,
            DeliveredAt = entity.DeliveredAt,
            Acknowledged = entity.IsAcknowledged,
            AcknowledgedAt = entity.AcknowledgedAt
        };
    }
}

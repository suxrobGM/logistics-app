using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums.Safety;
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
            TypeDisplay = GetAlertTypeDisplay(entity.AlertType),
            Status = entity.Status,
            StatusDisplay = GetAlertStatusDisplay(entity.Status),
            Source = entity.Source,
            SourceDisplay = GetAlertSourceDisplay(entity.Source),
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
            TypeDisplay = GetContactTypeDisplay(entity.ContactType),
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

    private static string GetAlertTypeDisplay(EmergencyAlertType type)
    {
        return type switch
        {
            EmergencyAlertType.PanicButton => "Panic Button",
            EmergencyAlertType.CrashDetected => "Crash Detected",
            EmergencyAlertType.RolloverDetected => "Rollover Detected",
            EmergencyAlertType.AirbagDeployed => "Airbag Deployed",
            EmergencyAlertType.MedicalEmergency => "Medical Emergency",
            EmergencyAlertType.SecurityThreat => "Security Threat",
            EmergencyAlertType.VehicleDisabled => "Vehicle Disabled",
            EmergencyAlertType.Other => "Other Emergency",
            _ => "Unknown"
        };
    }

    private static string GetAlertStatusDisplay(EmergencyAlertStatus status)
    {
        return status switch
        {
            EmergencyAlertStatus.Active => "Active",
            EmergencyAlertStatus.Acknowledged => "Acknowledged",
            EmergencyAlertStatus.Dispatching => "Dispatching Help",
            EmergencyAlertStatus.OnScene => "On Scene",
            EmergencyAlertStatus.Resolved => "Resolved",
            EmergencyAlertStatus.FalseAlarm => "False Alarm",
            _ => "Unknown"
        };
    }

    private static string GetAlertSourceDisplay(EmergencyAlertSource source)
    {
        return source switch
        {
            EmergencyAlertSource.DriverApp => "Driver App",
            EmergencyAlertSource.EldDevice => "ELD Device",
            EmergencyAlertSource.DispatcherInitiated => "Dispatcher Initiated",
            EmergencyAlertSource.AutomaticDetection => "Automatic Detection",
            _ => "Unknown"
        };
    }

    private static string GetContactTypeDisplay(EmergencyContactType type)
    {
        return type switch
        {
            EmergencyContactType.SafetyManager => "Safety Manager",
            EmergencyContactType.Dispatcher => "Dispatcher",
            EmergencyContactType.FleetManager => "Fleet Manager",
            EmergencyContactType.EmergencyServices => "Emergency Services",
            EmergencyContactType.FamilyMember => "Family Member",
            EmergencyContactType.Insurance => "Insurance Company",
            EmergencyContactType.TowService => "Tow Service",
            _ => "Unknown"
        };
    }
}

using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Shared.Models;

public class EmergencyAlertDto
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public Guid? TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public Guid? TripId { get; set; }

    public EmergencyAlertType Type { get; set; }
    public string TypeDisplay { get; set; } = string.Empty;
    public EmergencyAlertStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public EmergencyAlertSource Source { get; set; }
    public string SourceDisplay { get; set; } = string.Empty;

    public DateTime TriggeredAt { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }

    public string? DriverMessage { get; set; }
    public string? DispatcherNotes { get; set; }

    public Guid? AcknowledgedById { get; set; }
    public string? AcknowledgedByName { get; set; }
    public DateTime? AcknowledgedAt { get; set; }

    public Guid? ResolvedById { get; set; }
    public string? ResolvedByName { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    public List<EmergencyContactNotificationDto> Notifications { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}

public class EmergencyContactDto
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }

    public string Name { get; set; } = string.Empty;
    public EmergencyContactType Type { get; set; }
    public string TypeDisplay { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }

    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public bool NotifyOnEmergency { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class EmergencyContactNotificationDto
{
    public Guid Id { get; set; }
    public Guid ContactId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;

    public NotificationMethod Method { get; set; }
    public DateTime SentAt { get; set; }
    public bool Delivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool Acknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? FailureReason { get; set; }
}

public class EmergencySummaryDto
{
    public int ActiveAlerts { get; set; }
    public int TotalAlertsToday { get; set; }
    public int TotalAlertsThisWeek { get; set; }
    public int TotalAlertsThisMonth { get; set; }
    public int FalseAlarms { get; set; }
    public double AverageResponseTimeMinutes { get; set; }
}

#region Request DTOs

public class TriggerEmergencyAlertRequest
{
    public required Guid DriverId { get; set; }
    public Guid? TruckId { get; set; }
    public Guid? TripId { get; set; }
    public required EmergencyAlertType Type { get; set; }
    public required EmergencyAlertSource Source { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public string? DriverMessage { get; set; }
}

public class AcknowledgeEmergencyAlertRequest
{
    public required Guid AlertId { get; set; }
    public required Guid AcknowledgedById { get; set; }
    public string? DispatcherNotes { get; set; }
}

public class ResolveEmergencyAlertRequest
{
    public required Guid AlertId { get; set; }
    public required Guid ResolvedById { get; set; }
    public required string ResolutionNotes { get; set; }
    public bool IsFalseAlarm { get; set; }
}

public class CreateEmergencyContactRequest
{
    public Guid? EmployeeId { get; set; }
    public required string Name { get; set; }
    public required EmergencyContactType Type { get; set; }
    public required string Phone { get; set; }
    public string? Email { get; set; }
    public int Priority { get; set; } = 1;
    public bool NotifyOnEmergency { get; set; } = true;
    public string? Notes { get; set; }
}

#endregion

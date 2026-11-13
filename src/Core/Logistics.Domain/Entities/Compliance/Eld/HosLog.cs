using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class HosLog : Entity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    /// <summary>
    /// The date of this log entry (for daily grouping)
    /// </summary>
    public required DateTime LogDate { get; set; }

    /// <summary>
    /// The duty status during this log period
    /// </summary>
    public required DutyStatus DutyStatus { get; set; }

    /// <summary>
    /// When this duty status period started
    /// </summary>
    public required DateTime StartTime { get; set; }

    /// <summary>
    /// When this duty status period ended (null if still ongoing)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Duration of this status period in minutes
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Location description during this log period
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// GPS latitude of the location
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// GPS longitude of the location
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Optional remark or annotation for this log entry
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// The log entry ID in the ELD provider's system
    /// </summary>
    public string? ExternalLogId { get; set; }

    /// <summary>
    /// Which ELD provider this log came from
    /// </summary>
    public EldProviderType ProviderType { get; set; }
}

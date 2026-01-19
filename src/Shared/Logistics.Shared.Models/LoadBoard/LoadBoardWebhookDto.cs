using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Models;

public record LoadBoardWebhookResultDto
{
    public required LoadBoardWebhookEventType EventType { get; set; }
    public string? ExternalListingId { get; set; }
    public string? ExternalPostId { get; set; }
    public object? Data { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum LoadBoardWebhookEventType
{
    [Description("Unknown")] [EnumMember(Value = "unknown")]
    Unknown,

    [Description("Load Posted")] [EnumMember(Value = "load_posted")]
    LoadPosted,

    [Description("Load Cancelled")] [EnumMember(Value = "load_cancelled")]
    LoadCancelled,

    [Description("Load Expired")] [EnumMember(Value = "load_expired")]
    LoadExpired,

    [Description("Truck Post Expired")] [EnumMember(Value = "truck_post_expired")]
    TruckPostExpired,

    [Description("Rate Updated")] [EnumMember(Value = "rate_updated")]
    RateUpdated,

    [Description("Status Changed")] [EnumMember(Value = "status_changed")]
    StatusChanged
}

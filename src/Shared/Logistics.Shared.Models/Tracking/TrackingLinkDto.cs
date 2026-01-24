namespace Logistics.Shared.Models;

/// <summary>
/// Tracking link details for staff management.
/// </summary>
public record TrackingLinkDto
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Guid LoadId { get; set; }
    public long LoadNumber { get; set; }
    public string? LoadName { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public int AccessCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

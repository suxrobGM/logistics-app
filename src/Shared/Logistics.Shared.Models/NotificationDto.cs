namespace Logistics.Shared.Models;

public class NotificationDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
}

namespace Logistics.Shared.Models;

public record SetDeviceToken
{
    public Guid? UserId { get; set; }
    public string? DeviceToken { get; set; }
}

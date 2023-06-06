namespace Logistics.Client.Models;

public record CreateEmployee
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}

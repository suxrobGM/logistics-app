namespace Logistics.Shared.Models;

public record CreateEmployeeCommand
{
    public string? UserId { get; set; }
    public string? Role { get; set; }
}

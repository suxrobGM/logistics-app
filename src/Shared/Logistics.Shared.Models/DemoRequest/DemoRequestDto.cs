namespace Logistics.Shared.Models;

public record DemoRequestDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? FleetSize { get; init; }
    public string? Message { get; init; }
    public string? Notes { get; init; }
    public int Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

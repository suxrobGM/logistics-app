using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record ContactSubmissionDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public ContactSubject Subject { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public ContactSubmissionStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

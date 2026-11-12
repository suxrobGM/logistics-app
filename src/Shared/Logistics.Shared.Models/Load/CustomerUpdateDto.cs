using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record CustomerUpdateDto
{
    public CustomerUpdateEvent Event { get; init; }
    public CustomerUpdateOutcome Outcome { get; init; }
    public DateTime At { get; init; }
    public int RecipientCount { get; init; }
    public string? Reason { get; init; }
}

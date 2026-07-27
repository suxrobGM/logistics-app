namespace Logistics.Shared.Models;

public record OnboardingStepDto
{
    public required string Key { get; init; }
    public bool IsComplete { get; init; }
}

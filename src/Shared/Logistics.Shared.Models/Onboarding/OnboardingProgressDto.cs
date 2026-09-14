namespace Logistics.Shared.Models;

public record OnboardingProgressDto
{
    public bool IsSolo { get; init; }
    public List<OnboardingStepDto> Steps { get; init; } = [];
}

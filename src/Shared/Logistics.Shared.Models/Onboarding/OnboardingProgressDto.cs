namespace Logistics.Shared.Models;

public record OnboardingProgressDto
{
    public List<OnboardingStepDto> Steps { get; init; } = [];
}

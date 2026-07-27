using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record OnboardingProgressDto
{
    public OperatingMode OperatingMode { get; init; }
    public List<OnboardingStepDto> Steps { get; init; } = [];
}

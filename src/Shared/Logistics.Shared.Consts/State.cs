namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// US State
/// </summary>
/// <param name="DisplayName">Display name of the state</param>
/// <param name="Code">State code</param>
public record State(string DisplayName, string Code);
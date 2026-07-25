namespace Logistics.Shared.Models;

/// <summary>
/// A selectable LLM model for the admin global AI settings dropdown.
/// </summary>
public record LlmModelOptionDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}

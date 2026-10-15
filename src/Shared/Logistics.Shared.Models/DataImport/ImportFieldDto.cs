namespace Logistics.Shared.Models;

/// <summary>
///     A target field a spreadsheet column can be mapped to.
/// </summary>
public record ImportFieldDto
{
    public required string Key { get; init; }
    public required string Label { get; init; }
    public bool Required { get; init; }
}

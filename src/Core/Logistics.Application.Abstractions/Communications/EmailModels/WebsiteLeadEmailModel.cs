namespace Logistics.Application.Abstractions.Communications.EmailModels;

/// <summary>
///     A demo request or contact message from the public website, forwarded to the platform owner.
/// </summary>
public record WebsiteLeadEmailModel
{
    public required string Heading { get; init; }
    public required IReadOnlyList<WebsiteLeadField> Fields { get; init; }
    public string? Message { get; init; }
}

public record WebsiteLeadField(string Label, string Value);

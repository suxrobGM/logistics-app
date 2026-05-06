namespace Logistics.Shared.Models;

public record StripeTaxConfigDto
{
    public required string DefaultTaxCode { get; init; }
    public IReadOnlyList<StripeTaxRegistrationDto> Registrations { get; init; } = [];
    public IReadOnlyList<StripeTaxCodeDto> TaxCodes { get; init; } = [];
}

public sealed record StripeTaxRegistrationDto(
    string Id, string Country, string? State, string Status, DateTime? ActiveFrom, DateTime? ExpiresAt);

public sealed record StripeTaxCodeDto(string Id, string Name, string? Description);

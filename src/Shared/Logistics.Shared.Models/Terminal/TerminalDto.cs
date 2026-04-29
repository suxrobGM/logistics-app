using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public class TerminalDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string CountryCode { get; set; }
    public TerminalType Type { get; set; }
    public required Address Address { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

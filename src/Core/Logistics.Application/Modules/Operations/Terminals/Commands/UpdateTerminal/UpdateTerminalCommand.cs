using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Commands;

public class UpdateTerminalCommand : ICommand<Result>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? CountryCode { get; set; }
    public TerminalType? Type { get; set; }
    public Address? Address { get; set; }
    public string? Notes { get; set; }
}

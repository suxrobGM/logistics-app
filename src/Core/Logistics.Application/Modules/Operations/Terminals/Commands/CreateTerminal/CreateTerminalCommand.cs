using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Terminals.Commands;

public class CreateTerminalCommand : ICommand<Result<TerminalDto>>
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string CountryCode { get; set; } = null!;
    public TerminalType Type { get; set; }
    public Address Address { get; set; } = null!;
    public string? Notes { get; set; }
}

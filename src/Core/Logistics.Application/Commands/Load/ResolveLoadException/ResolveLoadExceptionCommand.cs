using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class ResolveLoadExceptionCommand : ICommand
{
    public Guid LoadId { get; set; }
    public Guid ExceptionId { get; set; }
    public required string Resolution { get; set; }
}

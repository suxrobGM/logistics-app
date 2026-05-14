using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class ProcessStripEventCommand : ICommand
{
    public string? RequestBodyJson { get; set; }
    public string? StripeSignature { get; set; }
}

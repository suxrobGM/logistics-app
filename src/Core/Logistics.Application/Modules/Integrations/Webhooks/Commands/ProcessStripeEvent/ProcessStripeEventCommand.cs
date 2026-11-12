namespace Logistics.Application.Modules.Integrations.Webhooks.Commands;

public class ProcessStripeEventCommand : ICommand
{
    public required string RequestBodyJson { get; set; }
    public required string StripeSignature { get; set; }
}

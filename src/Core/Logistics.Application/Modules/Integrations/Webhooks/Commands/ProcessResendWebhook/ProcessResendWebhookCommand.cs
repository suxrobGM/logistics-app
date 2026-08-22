using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Webhooks.Commands;

public class ProcessResendWebhookCommand : ICommand<Result>
{
    public required string RawBody { get; set; }
    public string? SvixId { get; set; }
    public string? SvixTimestamp { get; set; }
    public string? SvixSignature { get; set; }
}

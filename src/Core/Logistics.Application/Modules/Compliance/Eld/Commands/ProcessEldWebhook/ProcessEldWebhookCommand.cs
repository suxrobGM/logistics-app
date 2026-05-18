using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

public class ProcessEldWebhookCommand : ICommand
{
    public EldProviderType ProviderType { get; set; }
    public required string RequestBodyJson { get; set; }
    public string? Signature { get; set; }
}

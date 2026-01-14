using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class ProcessEldWebhookCommand : IAppRequest
{
    public EldProviderType ProviderType { get; set; }
    public required string RequestBodyJson { get; set; }
    public string? Signature { get; set; }
}

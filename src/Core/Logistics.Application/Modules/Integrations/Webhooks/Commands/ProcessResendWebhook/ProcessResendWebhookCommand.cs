using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Webhooks.Commands;

public class ProcessResendWebhookCommand : ICommand<Result<ResendWebhookOutcome>>
{
    public required string RawBody { get; set; }
    public string? SvixId { get; set; }
    public string? SvixTimestamp { get; set; }
    public string? SvixSignature { get; set; }
}

/// <summary>
/// What the endpoint should answer. Anything the provider must not retry is
/// <see cref="Accepted"/> - including events we deliberately ignore.
/// </summary>
public enum ResendWebhookOutcome
{
    Accepted,
    BadSignature,

    /// <summary>A downstream call failed. Nothing was recorded, so a retry is safe and wanted.</summary>
    Transient
}

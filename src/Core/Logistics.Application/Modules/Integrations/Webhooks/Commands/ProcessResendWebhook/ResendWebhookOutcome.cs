namespace Logistics.Application.Modules.Integrations.Webhooks.Commands;

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

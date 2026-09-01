namespace Logistics.Application.Modules.Integrations.Webhooks.Services;

/// <summary>
/// Remembers which webhook deliveries have already been handled, so a provider retrying the same
/// delivery becomes a no-op. Rows live in the master DB and are pruned by
/// <see cref="IProcessedWebhookEventCleanupService"/>.
/// </summary>
public interface IWebhookEventTracker : IApplicationService
{
    /// <summary>Checks whether this delivery was handled before.</summary>
    /// <param name="provider">The sending service, for example "Stripe" or "Resend".</param>
    /// <param name="eventKey">The provider's event ID, or a hash of the body when it sends none.</param>
    Task<bool> WasAlreadyHandledAsync(string provider, string eventKey, CancellationToken ct = default);

    /// <summary>
    ///     Records the delivery and saves. Call this only after the work succeeded: a recorded key
    ///     turns a later retry into a no-op, so recording a failure would drop the event for good.
    /// </summary>
    Task MarkHandledAsync(string provider, string eventKey, CancellationToken ct = default);

    /// <summary>Builds a stable key from a request body, for providers that send no event ID.</summary>
    string BuildKeyFromBody(string requestBody);
}

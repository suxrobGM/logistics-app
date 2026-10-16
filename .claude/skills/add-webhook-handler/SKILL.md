---
name: add-webhook-handler
description: Add a new external webhook receiver (Stripe, ELD, custom integration). Use when wiring an inbound webhook from a third-party service. Codifies signature validation, idempotency, audit logging, and the WebhookController + Modules/Integrations/Webhooks/Commands pattern so security checks are not silently skipped.
---

# Add a Webhook Handler

Inbound webhooks from external services (Stripe, Samsara, Motive, etc.) terminate at `WebhookController` and dispatch into the application layer as commands. Webhook handlers are **security-sensitive** - every checklist item below exists because something broke when it was missed.

## Files that must change

1. `src/Presentation/Logistics.API/Controllers/WebhookController.cs` - endpoint route (`[Route("webhooks")]`)
2. `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Commands/Process{Provider}Webhook/` - command + handler (Stripe lives in `Modules/Integrations/Webhooks/Commands/`, ELD in `Modules/Compliance/Eld/Commands/`)
3. Signature validation with a `WebhookSignature.*` helper, in one of two existing shapes: a verifier port in `Application.Abstractions` implemented in infrastructure (`IInboundEmailWebhookVerifier` → `ResendWebhookVerifier`), or the family's provider service verifies its own webhooks (ELD `{Name}EldService`)
4. Webhook secret - per-tenant integrations keep it on the `{Family}ProviderConfiguration` entity (`EldProviderConfiguration.WebhookSecret`); `appsettings.json` (env var, never committed) only for install-wide vendors (Stripe, Resend)
5. `…/Process{Provider}Webhook/Process{Provider}WebhookValidator.cs` - schema validation (skip if the only rule is `Id NotEmpty`)
6. Tests - at minimum: bad signature returns 400, replay returns 200 idempotent

## Step-by-step

### 1. Decide signature scheme

Most providers use HMAC of the raw body with a shared secret, sent in a header:

| Provider | Header | Algorithm |
| --- | --- | --- |
| Stripe | `Stripe-Signature` | HMAC-SHA256 with timestamp |
| Samsara | `X-Samsara-Signature` + `X-Samsara-Timestamp` | `v1=` hex HMAC-SHA256 of `v1:{timestamp}:{body}`, base64-decoded secret (`WebhookSignature.VerifySamsara`) |
| Motive | `X-Motive-Signature` | HMAC-SHA256 |
| Resend (Svix) | `svix-id` + `svix-timestamp` + `svix-signature` | base64 HMAC-SHA256 of `{id}.{timestamp}.{body}`, `whsec_` secret (`WebhookSignature.VerifySvix`) |

For a new provider, find their signature spec in their docs **before** writing any code. If they don't sign webhooks, push back - unsigned webhooks are spoofable and should not be wired in without an alternative (mutual TLS, allowlisted source IPs).

### 2. Read the raw body

Webhook signature validation is over the **raw bytes**, not the deserialized DTO. ASP.NET Core has already buffered the body by the time you read it; the controller pattern is:

```csharp
[HttpPost("provider")]
[AllowAnonymous]                // signature IS the auth
public async Task<IActionResult> Provider(CancellationToken ct)
{
    Request.EnableBuffering();   // re-readable stream
    using var reader = new StreamReader(Request.Body);
    var rawBody = await reader.ReadToEndAsync(ct);
    var signature = Request.Headers["X-Provider-Signature"].ToString();

    var result = await mediator.Send(new ProviderWebhookCommand
    {
        RawBody = rawBody,
        Signature = signature
    }, ct);

    return result.IsSuccess ? Ok() : BadRequest();
}
```

`[AllowAnonymous]` is correct here - the signature replaces auth.

### 3. Verify signature in the service layer

Do **not** hand-roll HMAC + comparison. Use `WebhookSignature.VerifyHmacSha256` from
`Logistics.Infrastructure.Integrations.Common` - it computes the lowercase-hex HMAC-SHA256 of the raw body and
compares with `CryptographicOperations.FixedTimeEquals`, and it **fails closed** (returns `false` when either the
secret or the signature is missing).

Two shapes exist - copy whichever fits:

- **Install-wide vendor** (one secret for the whole install): a verifier port in
  `Application.Abstractions`, implemented in infrastructure. `IInboundEmailWebhookVerifier` →
  `Infrastructure.Communications/Email/ResendWebhookVerifier.cs`:

  ```csharp
  internal sealed class ResendWebhookVerifier(IOptions<ResendOptions> options) : IInboundEmailWebhookVerifier
  {
      public bool Verify(string rawBody, string? messageId, string? timestamp, string? signature) =>
          WebhookSignature.VerifySvix(rawBody, messageId, timestamp, signature, options.Value.WebhookSecret);
  }
  ```

- **Per-tenant integration**: the family's provider service verifies against the tenant's stored
  secret (`SamsaraEldService` → `VerifySamsara`, `MotiveEldService` → `VerifyHmacSha256`).

Never compare signatures with `==` or `string.Equals` - they leak timing information and enable forgery.
Timestamped envelopes are already covered (`VerifySamsara`, `VerifySvix`); if a provider signs with a
scheme no helper covers, extend `WebhookSignature` rather than open-coding a compare.

For Stripe specifically, use `EventUtility.ConstructEvent(rawBody, signature, secret)` from the Stripe SDK - it
handles timestamp tolerance and constant-time compare for you.

### 4. Handler: validate, idempotency, dispatch

Do not hand-roll idempotency. Inject `IWebhookEventTracker`
(`Logistics.Application/Modules/Integrations/Webhooks/Services/`) and call `WasAlreadyHandledAsync`
then `MarkHandledAsync`. It owns the master-DB `ProcessedWebhookEvent` row (`Provider`, `EventKey`,
`ReceivedAt`, unique on `(Provider, EventKey)`) and its own `SaveChangesAsync`. `EventKey` is the
provider's event id when present, otherwise `BuildKeyFromBody(rawBody)` so a re-sent payload maps to
the same key.

```csharp
internal sealed class ProviderWebhookHandler(
    IProviderWebhookVerifier webhookVerifier,
    ITenantUnitOfWork tenantUow,
    IWebhookEventTracker webhookEvents,
    ILogger<ProviderWebhookHandler> logger)
    : IRequestHandler<ProviderWebhookCommand, Result>
{
    public async Task<Result> Handle(ProviderWebhookCommand cmd, CancellationToken ct)
    {
        // 1. Verify signature FIRST, before parsing
        if (!webhookVerifier.Verify(cmd.RawBody, cmd.Signature))
        {
            logger.LogWarning("Webhook signature verification failed");
            return Result.Fail("Invalid signature");   // controller maps to 400
        }

        // 2. Parse only after verification
        var payload = JsonSerializer.Deserialize<ProviderEvent>(cmd.RawBody)
            ?? throw new InvalidOperationException("Invalid payload");

        // 3. Idempotency: providers retry. Provider event id, else a hash of the raw body.
        const string provider = "Provider";
        var eventKey = payload.Id ?? webhookEvents.BuildKeyFromBody(cmd.RawBody);

        if (await webhookEvents.WasAlreadyHandledAsync(provider, eventKey, ct))
        {
            logger.LogInformation("Duplicate {Provider} webhook '{EventKey}' ignored", provider, eventKey);
            return Result.Ok();   // 200 - provider stops retrying
        }

        // 4. Do the work by event type, then persist tenant changes
        switch (payload.Type)
        {
            case "thing.created":
                await mediator.Send(new HandleProviderThingCreatedCommand(payload), ct);
                break;
            // …
            default:
                logger.LogInformation("Unhandled webhook event type {Type}", payload.Type);
                break;
        }
        await tenantUow.SaveChangesAsync(ct);

        // 5. Record the delivery so a retry of the same event becomes a no-op
        await webhookEvents.MarkHandledAsync(provider, eventKey, ct);

        return Result.Ok();
    }
}
```

Order matters:

1. **Signature first** - never parse untrusted bytes before verification.
2. **Then idempotency** - same `(Provider, EventKey)` arriving twice is a no-op returning 200.
3. **Dispatch** by event type; unknown event types should log and return 200, not crash.
4. **Record the event key** after the work so a retry short-circuits. The unique `(Provider, EventKey)` index is the backstop if two deliveries race.

`WebhookEventCleanupJob` prunes old ledger rows daily, so the table doesn't grow unbounded.

### 5. Config

**Per-tenant integration** (ELD and the like): the secret lives on the tenant's
`{Family}ProviderConfiguration.WebhookSecret` (`EldProviderConfiguration.WebhookSecret` today), entered
through the connect dialog. No `appsettings` entry.

**Install-wide vendor** (Stripe, Resend): declare `ProviderOptions:WebhookSecret` as an empty string in
`appsettings.json`; the real value comes from the env var `ProviderOptions__WebhookSecret`. **Never
commit a real secret.**

### 6. Tests

Four cases, minimum: bad signature → error; the same payload twice → both 200 with the side effect
running once; unknown event type → logged, 200, no crash; happy path → command dispatched.

## Verification checklist

- [ ] Endpoint added to `WebhookController` with `[AllowAnonymous]` and raw-body reading
- [ ] Signature verified with a `WebhookSignature.*` helper (or provider SDK's verify) - fails closed
- [ ] Signature verified **before** payload parsing
- [ ] Idempotency via `IWebhookEventTracker`; `EventKey` = provider event id, else `BuildKeyFromBody`
- [ ] Event key recorded in the master-DB ledger after the work runs
- [ ] Unknown event types log and return 200
- [ ] Webhook secret on the tenant's provider configuration (per-tenant) or from an env var (install-wide), never committed
- [ ] Tests cover: bad signature, replay, unknown event type, happy path
- [ ] Provider's webhook dashboard configured to point at the new endpoint
- [ ] Endpoint URL added to deployment docs / runbook

## Common mistakes

- **No idempotency** - providers retry on 5xx, you double-charge / double-create.
- **Returning non-200 for unknown event types** - provider keeps retrying forever.
- **Logging the raw body at INFO** - webhooks often contain PII (emails, names). Use DEBUG and scrub.

## Provider-specific notes

### Stripe

Use the Stripe SDK's built-in verifier. It validates both signature and timestamp tolerance:

```csharp
var stripeEvent = EventUtility.ConstructEvent(rawBody, stripeSignatureHeader, webhookSecret);
```

Stripe events have unique `id` fields - perfect for idempotency keys. Mount the endpoint at `/webhooks/stripe` (existing convention).

### ELD providers (Samsara, Motive, Geotab)

Mount at `/webhooks/eld/{provider}/{tenantId}` - the tenant registers its own URL (shown on the ELD providers page), and `ProcessEldWebhookHandler` switches to that tenant before verifying against its `EldProviderConfiguration.WebhookSecret`. The idempotency key is prefixed with the tenant id. Samsara signs a timestamped envelope (`VerifySamsara`); Motive and Geotab use the body-only `VerifyHmacSha256` (Motive's real scheme is unverified - its docs are not public).

### Calling back into the provider

When the handler needs to fetch more from the provider's API, use `TryGetFromJsonAsync` from `Logistics.Infrastructure.Integrations.Common` rather than hand-rolling a try/catch around `GetAsync` + `ReadFromJsonAsync`:

```csharp
var trip = await httpClient.TryGetFromJsonAsync<TripResponse>(url, logger, "fetch trip", ct: ct);
```

It logs and returns `default` on a non-success status, network error, or parse failure, so it never throws. Push paths that must surface an error to the caller are the exception - see the QuickBooks helpers in `Integrations.Accounting`, which throw on purpose.

## Related

- `.claude/rules/backend/api-design.md` - security invariants and error-code conventions
- `add-integration-provider` - if the webhook comes with a new vendor integration
- `feature-map.md` → Operations / Financial / Compliance for the feature being webhooked
- `WebhookController.cs` - existing endpoints to copy from

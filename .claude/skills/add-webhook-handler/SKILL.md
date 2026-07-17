---
name: add-webhook-handler
description: Add a new external webhook receiver (Stripe, ELD, custom integration). Use when wiring an inbound webhook from a third-party service. Codifies signature validation, idempotency, audit logging, and the WebhookController + Modules/Integrations/Webhooks/Commands pattern so security checks are not silently skipped.
---

# Add a Webhook Handler

Inbound webhooks from external services (Stripe, Samsara, Motive, etc.) terminate at `WebhookController` and dispatch into the application layer via MediatR commands. Webhook handlers are **security-sensitive** — every checklist item below exists because something broke when it was missed.

## Files that must change

1. `src/Presentation/Logistics.API/Controllers/WebhookController.cs` — endpoint route (`[Route("webhooks")]`)
2. `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Commands/Process{Provider}Webhook/` — command + handler (Stripe lives in `Modules/Integrations/Webhooks/Commands/`, ELD in `Modules/Compliance/Eld/Commands/`)
3. `src/Infrastructure/Logistics.Infrastructure.{Module}/{Provider}/{Provider}WebhookService.cs` — signature validation (use `WebhookSignature.VerifyHmacSha256`)
4. `appsettings.json` — webhook secret config (matched by env var, never committed)
5. `…/Process{Provider}Webhook/Process{Provider}WebhookValidator.cs` — schema validation (skip if the only rule is `Id NotEmpty`)
6. Tests — at minimum: bad signature returns 400, replay returns 200 idempotent

## Step-by-step

### 1. Decide signature scheme

Most providers use HMAC of the raw body with a shared secret, sent in a header:

| Provider | Header                | Algorithm                  |
| -------- | --------------------- | -------------------------- |
| Stripe   | `Stripe-Signature`    | HMAC-SHA256 with timestamp |
| Samsara  | `X-Samsara-Signature` | HMAC-SHA256                |
| Motive   | `X-Motive-Signature`  | HMAC-SHA256                |

For a new provider, find their signature spec in their docs **before** writing any code. If they don't sign webhooks, push back — unsigned webhooks are spoofable and should not be wired in without an alternative (mutual TLS, allowlisted source IPs).

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

`[AllowAnonymous]` is correct here — the signature replaces auth.

### 3. Verify signature in the service layer

Do **not** hand-roll HMAC + comparison. Use `WebhookSignature.VerifyHmacSha256` from
`Logistics.Infrastructure.Integrations.Common` — it computes the lowercase-hex HMAC-SHA256 of the raw body and
compares with `CryptographicOperations.FixedTimeEquals`, and it **fails closed** (returns `false` when either the
secret or the signature is missing).

`Infrastructure.{Module}/{Provider}/{Provider}WebhookService.cs`:

```csharp
internal sealed class ProviderWebhookService(IOptions<ProviderOptions> options)
{
    public bool VerifySignature(string rawBody, string? signatureHex) =>
        WebhookSignature.VerifyHmacSha256(rawBody, signatureHex, options.Value.WebhookSecret);
}
```

Never compare signatures with `==` or `string.Equals` — they leak timing information and enable forgery. If a
provider signs with a scheme the helper doesn't cover (e.g. a timestamped envelope), extend the helper rather
than open-coding a compare.

For Stripe specifically, use `EventUtility.ConstructEvent(rawBody, signature, secret)` from the Stripe SDK — it
handles timestamp tolerance and constant-time compare for you.

### 4. Handler: validate, idempotency, dispatch

Idempotency lives in the **master DB** via the `ProcessedWebhookEvent` ledger (entity in
`Logistics.Domain.Entities`): columns `Provider`, `EventKey`, `ReceivedAt`, with a unique index on
`(Provider, EventKey)`. It is a dedupe ledger, not a raw-body store. `EventKey` is the provider's event id when
present, otherwise a SHA-256 hash of the raw body so a re-sent payload maps to the same key.

```csharp
internal sealed class ProviderWebhookHandler(
    IProviderWebhookService webhookService,
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow,
    ILogger<ProviderWebhookHandler> logger)
    : IRequestHandler<ProviderWebhookCommand, Result>
{
    public async Task<Result> Handle(ProviderWebhookCommand cmd, CancellationToken ct)
    {
        // 1. Verify signature FIRST, before parsing
        if (!webhookService.VerifySignature(cmd.RawBody, cmd.Signature))
        {
            logger.LogWarning("Webhook signature verification failed");
            return Result.Fail("Invalid signature");   // controller maps to 400
        }

        // 2. Parse only after verification
        var payload = JsonSerializer.Deserialize<ProviderEvent>(cmd.RawBody)
            ?? throw new InvalidOperationException("Invalid payload");

        // 3. Idempotency: providers retry. Provider event id, else a hash of the raw body.
        const string provider = "Provider";
        var eventKey = payload.Id
            ?? Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(cmd.RawBody)));

        var alreadyProcessed = await masterUow.Repository<ProcessedWebhookEvent>()
            .GetAsync(e => e.Provider == provider && e.EventKey == eventKey, ct);
        if (alreadyProcessed is not null)
        {
            logger.LogInformation("Duplicate {Provider} webhook '{EventKey}' ignored", provider, eventKey);
            return Result.Ok();   // 200 — provider stops retrying
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
        await masterUow.Repository<ProcessedWebhookEvent>()
            .AddAsync(new ProcessedWebhookEvent { Provider = provider, EventKey = eventKey }, ct);
        await masterUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
```

Order matters:

1. **Signature first** — never parse untrusted bytes before verification.
2. **Then idempotency** — same `(Provider, EventKey)` arriving twice is a no-op returning 200.
3. **Dispatch** by event type; unknown event types should log and return 200, not crash.
4. **Record the event key** after the work so a retry short-circuits. The unique `(Provider, EventKey)` index is the backstop if two deliveries race.

`WebhookEventCleanupJob` prunes old ledger rows daily, so the table doesn't grow unbounded.

### 5. Config

`appsettings.json`:

```json
{
  "ProviderOptions": {
    "WebhookSecret": ""
  }
}
```

Real value comes from env var: `ProviderOptions__WebhookSecret`. **Never commit a real secret.**

### 6. Tests

Minimum coverage:

```csharp
public class ProviderWebhookHandlerTests
{
    [Fact]
    public async Task Handle_BadSignature_ReturnsError() { /* signature fails → 400 */ }

    [Fact]
    public async Task Handle_ReplayedEvent_IsIdempotent() { /* same payload twice → both 200, side effect runs once */ }

    [Fact]
    public async Task Handle_UnknownEventType_LogsAndReturnsSuccess() { /* no crash */ }

    [Fact]
    public async Task Handle_ValidEvent_DispatchesCommand() { /* happy path */ }
}
```

## Verification checklist

- [ ] Endpoint added to `WebhookController` with `[AllowAnonymous]` and raw-body reading
- [ ] Signature verified with `WebhookSignature.VerifyHmacSha256` (or provider SDK's verify) — fails closed
- [ ] Signature verified **before** payload parsing
- [ ] Idempotency via `ProcessedWebhookEvent` `(Provider, EventKey)`; `EventKey` = provider event id, else SHA-256 body hash
- [ ] Event key recorded in the master-DB ledger after the work runs
- [ ] Unknown event types log and return 200
- [ ] Webhook secret loaded from env var, not appsettings.json
- [ ] Tests cover: bad signature, replay, unknown event type, happy path
- [ ] Provider's webhook dashboard configured to point at the new endpoint
- [ ] Endpoint URL added to deployment docs / runbook

## Common mistakes

- **Parsing JSON before verifying signature** — opens an attack surface for malformed-payload exploits.
- **Using `==` to compare signatures** — timing attacks let attackers forge valid signatures one byte at a time.
- **No idempotency** — providers retry on 5xx, you double-charge / double-create.
- **Returning non-200 for unknown event types** — provider keeps retrying forever.
- **Logging the raw body at INFO** — webhooks often contain PII (emails, names). Use DEBUG and scrub.
- **Storing the secret in appsettings.json** — it ends up in git history. Always env var or Key Vault.

## Provider-specific notes

### Stripe

Use the Stripe SDK's built-in verifier. It validates both signature and timestamp tolerance:

```csharp
var stripeEvent = EventUtility.ConstructEvent(rawBody, stripeSignatureHeader, webhookSecret);
```

Stripe events have unique `id` fields — perfect for idempotency keys. Mount the endpoint at `/webhooks/stripe` (existing convention).

### Samsara / Motive (ELD)

Both use `X-{Provider}-Signature` HMAC-SHA256. Mount at `/webhooks/eld/{provider}`. The signed payload contains the tenant's ELD account, but you'll typically need to look up the `EldProviderConfiguration` row to map back to a tenant.

### Calling back into the provider

When the handler needs to fetch more from the provider's API, use `TryGetFromJsonAsync` from `Logistics.Infrastructure.Integrations.Common` rather than hand-rolling a try/catch around `GetAsync` + `ReadFromJsonAsync`:

```csharp
var trip = await httpClient.TryGetFromJsonAsync<TripResponse>(url, logger, "fetch trip", ct: ct);
```

It logs and returns `default` on a non-success status, network error, or parse failure, so it never throws. Push paths that must surface an error to the caller are the exception — see the QuickBooks helpers in `Integrations.Accounting`, which throw on purpose.

## Related

- `.claude/rules/backend/security.md` — webhook signature validation rule
- `feature-map.md` → Operations / Financial / Compliance for the feature being webhooked
- `WebhookController.cs` — existing endpoints to copy from

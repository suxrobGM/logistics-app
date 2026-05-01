---
name: add-webhook-handler
description: Add a new external webhook receiver (Stripe, ELD, custom integration). Use when wiring an inbound webhook from a third-party service. Codifies signature validation, idempotency, audit logging, and the WebhookController + Application/Commands/Webhooks pattern so security checks are not silently skipped.
---

# Add a Webhook Handler

Inbound webhooks from external services (Stripe, Samsara, Motive, etc.) terminate at `WebhookController` and dispatch into the application layer via MediatR commands. Webhook handlers are **security-sensitive** — every checklist item below exists because something broke when it was missed.

## Files that must change

1. `src/Presentation/Logistics.API/Controllers/WebhookController.cs` — endpoint route
2. `src/Core/Logistics.Application/Commands/Webhooks/{Provider}{Event}Command.cs` + handler
3. `src/Infrastructure/Logistics.Infrastructure.{Module}/{Provider}/{Provider}WebhookService.cs` — signature validation
4. `appsettings.json` — webhook secret config (matched by env var, never committed)
5. `src/Core/Logistics.Application/Commands/Webhooks/{Provider}{Event}Validator.cs` — schema validation
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

    return result.Success ? Ok() : BadRequest();
}
```

`[AllowAnonymous]` is correct here — the signature replaces auth.

### 3. Verify signature in the service layer (constant-time compare)

`Infrastructure.{Module}/{Provider}/{Provider}WebhookService.cs`:

```csharp
internal sealed class ProviderWebhookService(IOptions<ProviderOptions> options)
{
    public bool VerifySignature(string rawBody, string signature)
    {
        var secret = options.Value.WebhookSecret;
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(signature))
            return false;

        var computed = HmacSha256Hex(secret, rawBody);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(signature));
    }

    private static string HmacSha256Hex(string secret, string body)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
```

**Always use `CryptographicOperations.FixedTimeEquals`** — `==` and `string.Equals` leak timing information and enable signature forgery.

For Stripe specifically, use `Stripe.Webhooks.ConstructEvent(rawBody, signature, secret)` from the Stripe SDK — it handles timestamp tolerance and constant-time compare for you.

### 4. Handler: validate, idempotency, dispatch

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
            return Result.CreateError("Invalid signature");
        }

        // 2. Parse only after verification
        var payload = JsonSerializer.Deserialize<ProviderEvent>(cmd.RawBody)
            ?? throw new InvalidOperationException("Invalid payload");

        // 3. Idempotency: providers retry. Use the event ID as a unique key.
        var alreadyProcessed = await masterUow.Repository<ProcessedWebhook>()
            .AnyAsync(w => w.ProviderId == payload.Id && w.Provider == "Provider", ct);
        if (alreadyProcessed)
        {
            logger.LogInformation("Webhook {Id} already processed, skipping", payload.Id);
            return Result.CreateSuccess(); // 200 — provider stops retrying
        }

        // 4. Audit log BEFORE side effects (so we know we got it even if processing throws)
        await masterUow.Repository<ProcessedWebhook>().AddAsync(new ProcessedWebhook
        {
            ProviderId = payload.Id,
            Provider = "Provider",
            EventType = payload.Type,
            ReceivedAt = DateTime.UtcNow,
            RawBody = cmd.RawBody
        }, ct);
        await masterUow.SaveChangesAsync(ct);

        // 5. Dispatch domain action(s) by event type
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

        return Result.CreateSuccess();
    }
}
```

Order matters:

1. **Signature first** — never parse untrusted bytes before verification.
2. **Then idempotency** — same event arriving twice should be a no-op, returning 200.
3. **Audit log** before side effects so you have a record even if the side-effect throws.
4. **Dispatch** by event type; unknown event types should log and return 200, not crash.

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
- [ ] Signature verified using `CryptographicOperations.FixedTimeEquals` (or provider SDK's verify)
- [ ] Signature verified **before** payload parsing
- [ ] Idempotency check via stored event ID
- [ ] Audit log written before side effects
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

## Related

- `.claude/rules/backend/security.md` — webhook signature validation rule
- `feature-map.md` → Operations / Financial / Compliance for the feature being webhooked
- `WebhookController.cs` — existing endpoints to copy from

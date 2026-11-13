---
name: add-webhook-handler
description: Add an inbound webhook receiver (Stripe, ELD, Resend, a custom vendor). Use when a third-party service pushes events to us. Covers raw-body signature verification, the processed-event ledger, and where secrets live, so security checks are not silently skipped.
---

# Add a webhook handler

Webhooks land on `WebhookController` (`src/Presentation/Logistics.API/Controllers/Integrations/`, route `webhooks`) and dispatch a command. The order of checks in the handler is the security model.

## 1. Signature scheme

Read the vendor's signing spec first. If they don't sign, push back; unsigned webhooks are spoofable.

| Vendor | Header | Helper |
| --- | --- | --- |
| Stripe | `Stripe-Signature` | `StripeWebhookParser` (wraps the SDK; `Infrastructure.Payments`) |
| Samsara | `X-Samsara-Signature` + `X-Samsara-Timestamp` | `WebhookSignature.VerifySamsara` |
| Motive, Geotab | `X-Motive-Signature` etc. | `WebhookSignature.VerifyHmacSha256` |
| Resend (Svix) | `svix-id`, `svix-timestamp`, `svix-signature` | `SvixSignature.Verify` (`Infrastructure.Communications/Email/`) |

The helpers compare in constant time and fail closed. Never compare signatures with `==`. A new scheme extends `WebhookSignature` (`Infrastructure.Integrations/Common/`).

## 2. Controller

```csharp
[HttpPost("provider")]
[AllowAnonymous] // the signature is the auth
public async Task<IActionResult> Provider(CancellationToken ct)
{
    Request.EnableBuffering();
    using var reader = new StreamReader(Request.Body);
    var rawBody = await reader.ReadToEndAsync(ct);
    var signature = Request.Headers["X-Provider-Signature"].ToString();

    var result = await mediator.Send(new ProcessProviderWebhookCommand { RawBody = rawBody, Signature = signature }, ct);
    return result.IsSuccess ? Ok() : BadRequest();
}
```

Verify over the raw bytes, never a re-serialized DTO.

## 3. Verification shape

- **Install-wide vendor** (one secret): a verifier port in `Application.Abstractions`, implemented in infrastructure (`IInboundEmailWebhookVerifier` → `ResendWebhookVerifier`). Secret is an empty string in `appsettings.json`, real value from an env var.
- **Per-tenant integration**: the provider service verifies against `{Family}ProviderConfiguration.WebhookSecret` (`SamsaraEldService`). Route `/webhooks/eld/{provider}/{tenantId}`; the handler switches to that tenant first and prefixes the event key with the tenant id.

## 4. Handler

`Modules/{Module}/{Feature}/Commands/Process{Provider}Webhook/` (Stripe: `Modules/Integrations/Webhooks/Commands/`; ELD: `Modules/Compliance/Eld/Commands/`).

```csharp
internal sealed class ProcessProviderWebhookHandler(
    IProviderWebhookVerifier verifier,
    IWebhookEventTracker webhookEvents,
    IMediator mediator,
    ITenantUnitOfWork tenantUow,
    ILogger<ProcessProviderWebhookHandler> logger)
    : IRequestHandler<ProcessProviderWebhookCommand, Result>
{
    public async Task<Result> Handle(ProcessProviderWebhookCommand cmd, CancellationToken ct)
    {
        if (!verifier.Verify(cmd.RawBody, cmd.Signature))
        {
            logger.LogWarning("Provider webhook signature verification failed");
            return Result.Fail("Invalid signature");
        }

        var payload = JsonSerializer.Deserialize<ProviderEvent>(cmd.RawBody)!;
        var eventKey = payload.Id ?? webhookEvents.BuildKeyFromBody(cmd.RawBody);
        if (await webhookEvents.WasAlreadyHandledAsync("Provider", eventKey, ct))
        {
            return Result.Ok();
        }

        switch (payload.Type)
        {
            case "thing.created":
                await mediator.Send(new HandleThingCreatedCommand(payload), ct);
                break;
            default:
                logger.LogInformation("Unhandled provider webhook {Type}", payload.Type);
                break;
        }

        await tenantUow.SaveChangesAsync(ct);
        await webhookEvents.MarkHandledAsync("Provider", eventKey, ct);
        return Result.Ok();
    }
}
```

1. **Verify before parsing.** Never deserialize untrusted bytes first.
2. **Deduplicate** with `IWebhookEventTracker` (master-DB ledger, unique on provider + key, pruned daily). Vendors retry; a duplicate returns 200.
3. **Unknown event types log and return 200**, or the vendor retries forever.
4. **Mark handled after the work.**

Don't log raw bodies above Debug; they often contain PII. To call back into the vendor, use `TryGetFromJsonAsync`.

## 5. Tests and rollout

Test: bad signature → failure; same payload twice → side effect once; unknown type → 200; happy path. Then point the vendor dashboard at the endpoint and add the URL to the deployment docs.

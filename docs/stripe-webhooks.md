# Stripe Webhooks

The API exposes a single Stripe webhook receiver. It covers three payment surfaces: platform subscriptions, customer invoice payments, and Stripe Connect account status.

## Endpoint

| Environment | URL                                          |
| ----------- | -------------------------------------------- |
| Production  | `https://api.yourdomain.com/webhooks/stripe` |
| Local       | `https://localhost:7000/webhooks/stripe`     |

The route is `POST /webhooks/stripe` and is `[AllowAnonymous]` — authentication comes from the `Stripe-Signature` header, verified against the webhook signing secret before any payload is parsed.

## Events to Subscribe To

Subscribe to exactly these nine events. Anything else is accepted and ignored (returns `200`), but subscribing to events you don't handle just adds noise and retry traffic.

| Event                           | What the handler does                                                                |
| ------------------------------- | ------------------------------------------------------------------------------------ |
| `invoice.paid`                  | Records a `Payment` row for the tenant's subscription invoice                        |
| `customer.subscription.created` | Creates or updates the tenant's `Subscription`                                       |
| `customer.subscription.updated` | Syncs status, `CancelAtPeriodEnd`, and plan changes                                  |
| `customer.subscription.deleted` | Marks the subscription cancelled                                                     |
| `checkout.session.completed`    | Records a `Payment` against the invoice; applies it if the session already settled   |
| `payment_intent.processing`     | Moves the payment to `Pending` (delayed methods: ACH, SEPA, BACS)                    |
| `payment_intent.succeeded`      | Moves the payment to `Paid`                                                          |
| `payment_intent.payment_failed` | Moves the payment to `Failed` and appends the Stripe error to the description        |
| `account.updated`               | Syncs the tenant's `ConnectStatus`, `ChargesEnabled`, `PayoutsEnabled` (**Connect**) |

Every handler is keyed off `tenant_id` in Stripe object metadata. Objects without it are ignored rather than treated as errors — employee payout accounts, for example, carry no `tenant_id`.

## Two Endpoints Are Required

`account.updated` fires on a _connected_ account, not on the platform account. Stripe delivers connected-account events only to endpoints created with `connect: true`, and that is a distinct endpoint from your platform endpoint with a **distinct signing secret**.

So in the Stripe Dashboard you need two destinations, both pointing at the same URL:

1. **Platform events** — `invoice.paid`, `customer.subscription.*`, `checkout.session.completed`, `payment_intent.*`
2. **Connected account events** — `account.updated`

> **Known limitation.** `StripeOptions` exposes a single `WebhookSecret`, and `ProcessStripEventHandler` passes that one secret to `EventUtility.ConstructEvent`. Only one of the two endpoints can verify. The other returns `400`, and Stripe retries it for up to three days before giving up. Until `StripeOptions` grows a second secret (`ConnectWebhookSecret`) and the handler tries both, Connect status must be refreshed by polling `GET /stripe/connect/status` rather than by webhook.

## Configuration

```env
Stripe__SecretKey=sk_test_...
Stripe__WebhookSecret=whsec_...
```

Copy the signing secret from the endpoint's detail page in the Dashboard, or from `stripe listen` output when developing locally.

## Local Development

The Stripe CLI forwards to localhost and issues a single signing secret covering both platform and Connect events, so the two-endpoint problem above doesn't bite you locally.

```bash
stripe login
stripe listen --forward-to https://localhost:7000/webhooks/stripe
# prints: Ready! Your webhook signing secret is whsec_...
```

Put that `whsec_...` in `Stripe__WebhookSecret`, then fire test events:

```bash
stripe trigger checkout.session.completed
stripe trigger payment_intent.succeeded
stripe trigger invoice.paid
```

Triggered events carry no `tenant_id` metadata, so handlers will short-circuit. To exercise a handler end to end, drive the real flow (create a checkout session from the customer portal) and let Stripe emit the event naturally.

## Delivery Semantics

- **Retries.** A handler failure returns `400`, which Stripe treats as a failed delivery and retries with exponential backoff for up to three days.
- **Idempotency.** Stripe delivers at least once. `checkout.session.completed` bails if a `Payment` already exists for the session's payment intent, and the `payment_intent.*` handlers refuse to downgrade a terminal status (`Paid`, `Failed`, `Cancelled`, `Refunded`) back to `Pending`.
- **Ordering is not guaranteed.** `payment_intent.succeeded` can arrive before `checkout.session.completed`. Both paths converge on the same `Payment` row keyed by `StripePaymentIntentId`.
- **API version mismatches** are tolerated — `ConstructEvent` is called with `throwOnApiVersionMismatch: false`, so an account on a newer Stripe API version won't break delivery.

## Related Files

- Controller: `src/Presentation/Logistics.API/Controllers/WebhookController.cs`
- Handler: `src/Core/Logistics.Application/Modules/Integrations/Webhooks/Commands/ProcessStripEvent/ProcessStripEventHandler.cs`
- Options: `src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripeOptions.cs`
- Connect service: `src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripeConnectService.cs`
- See also: [Stripe Connect](stripe-connect.md), [Customer Payments](customer-payments.md)

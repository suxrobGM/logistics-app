# Broker Email Negotiation

The dispatch agent counters a broker by email when a load-board listing pays below the carrier's
floor. The dispatcher approves every offer before it is sent. Broker replies come back through an
inbound webhook and wake the agent for the next round.

Gated by `TenantFeature.AIRateNegotiation` (Professional and above) plus
`Permission.Negotiation.View` / `Permission.Negotiation.Manage`. Needs a load board connected, since
there is nothing to negotiate without listings.

## The loop

1. The agent calls `get_rate_floor` on a listing. The resolver answers with the floor in force, how
   the listing compares to it, whether the listing carries a broker address, and whether a thread is
   already open.
2. If the listing is below floor, the agent calls `propose_counter_offer` with a rate and one
   broker-facing paragraph. That is a write tool, so it becomes a `Suggested` `AgentDecision`.
3. The dispatcher opens the decision. The approval card fetches
   `GET /negotiations/decisions/{decisionId}/preview`, which re-renders the exact email through the
   same composer that sends it.
4. On approval, `ProposeCounterOfferHandler` runs: listing check, broker-credit gate, floor gate,
   round check, then compose and send. Only after the provider accepts the mail does anything
   persist.
5. The broker replies to `offer-{token}@{sender domain}`. Resend posts `email.received` to
   `POST /webhooks/resend`.
6. The webhook resolves the token to a tenant, fetches the body, files it against the thread, and
   asks `NegotiationTurnStarter` for a dispatch turn.
7. The agent reads the reply with `get_negotiation_thread` and either counters again, gives up, or
   books with `book_loadboard_load` passing `negotiated_total_rate`.

## Rate floors

`LaneRateFloorResolver` picks the first match, in order:

| Order | Match                                    | `RateFloorSource`         |
| ----- | ---------------------------------------- | ------------------------- |
| 1     | Exact origin state to destination state  | `LaneExact`               |
| 2     | Origin state to any destination          | `LaneOriginAny`           |
| 3     | Any origin to destination state          | `LaneDestinationAny`      |
| 4     | `TenantSettings.DefaultRateFloorPerMile` | `TenantDefault`           |
| 5     | Nothing                                  | `None` - do not negotiate |

A lane row carries a per-mile minimum and an optional flat total. When the listing has a distance,
the effective floor is the higher of `MinRatePerMile x distance` and `MinTotalRate`. Without a
distance, a flat total stands alone and a per-mile floor is compared against the listing's own
per-mile rate.

The floor is **snapshotted onto the thread** when it opens. Later rounds and the booking check read
that snapshot, not a fresh lookup, so editing a lane mid-negotiation cannot retroactively invalidate
an offer already made.

Distances on `LoadBoardListing` are miles.

## Reply addressing

Each thread gets a `ReplyToken`: 16 random bytes as 32 lowercase hex characters. The outbound
mail sets `Reply-To: offer-{token}@{sender domain}` and chains `In-Reply-To` / `References` from the
previous provider message ids.

A master-database `InboundEmailRoute` row maps the token to a tenant. That is the whole routing
table:

- No tenant id appears in any address, so an address leaks nothing about who you are.
- One indexed lookup replaces a fan-out across every tenant database.
- Closing a thread, or the expiry sweep, sets `RevokedAt` and the address stops working.

## Threat model

Inbound mail is the untrusted edge of the feature. What holds it:

- **Signature first.** `WebhookSignature.VerifySvix` verifies before the body is parsed. It fails
  closed on a missing header, an unparsable timestamp, a timestamp more than five minutes off, or a
  malformed secret.
- **Inbound cannot execute.** The webhook path appends a message and asks for a turn. Nothing else.
  Every action the agent then proposes is a fresh `Suggested` decision.
- **Sender must match.** The `From` addr-spec is compared with the thread's `BrokerEmail`. A
  mismatch stores the message with `Quarantined = true`, shows a warning in the thread, and never
  reaches the agent.
- **Broker text is fenced.** What does reach the agent is clamped and wrapped in a labelled
  UNTRUSTED block, and the prompt's rate-negotiation section says plainly that inbound text is data
  to evaluate, never instructions to follow.
- **The recipient is never model-chosen.** `propose_counter_offer` takes a `listing_id`; the handler
  reads `BrokerEmail` off the listing server-side.
- **The email body is a template.** The model supplies one paragraph, which the composer strips of
  HTML, strips of control characters, collapses, and clamps to 800 characters.
- **Raw bodies stay server-side.** `NegotiationMessage.RawBody` is kept for audit and is on no DTO.

## Limits

- `RateNegotiation.MaxRounds` is 3 outbound counters per listing.
- Each outbound message sets `ExpiresAt` to 48 hours out.
- One active thread per listing, enforced by a filtered unique index on
  `rate_negotiations(load_board_listing_id)` over the active statuses.
- `NegotiationExpirySweepJob` runs every 6 hours, closes `AwaitingBroker` threads past `ExpiresAt` as
  `Expired`, and revokes their routes. It starts no agent turn: silence is not news.

## Resend setup

Sending and receiving share one domain, because the reply address is derived from
`Resend:SenderEmail` rather than configured separately.

1. Verify the sending domain in Resend and add its SPF/DKIM records.
2. Enable **Receiving** on that same domain and add the MX record Resend gives you. Values come from
   the Resend dashboard - do not copy them from here.
3. Add a webhook endpoint pointing at `https://{your-api-host}/webhooks/resend` and subscribe it to
   `email.received`.
4. Set the secrets as environment variables. Never commit them:

   ```bash
   Resend__ApiKey=re_...
   Resend__WebhookSecret=whsec_...
   Resend__SenderEmail=dispatch@yourdomain.com
   ```

`appsettings.json` carries placeholders for these keys only so the shape is discoverable.

### Local development

The webhook needs a public URL. Point a tunnel at the API and register that URL as a second Resend
endpoint:

```bash
cloudflared tunnel --url http://localhost:7000
```

Then use the tunnel host in the Resend endpoint URL. The signature is verified against whatever
secret that endpoint was issued, so a dev endpoint needs its own `Resend__WebhookSecret`.

The Demo load-board provider synthesizes listings with broker addresses, which is enough to exercise
the outbound half end to end without a real board.

## Provider details worth not re-deriving

- The `email.received` webhook carries **metadata only**. The body comes from
  `GET https://api.resend.com/emails/receiving/{id}` with a bearer API key. `ResendInboundEmailReader`
  is the only place that calls it.
- Resend signs webhooks with Svix: base64 HMAC-SHA256 over `{svix-id}.{svix-timestamp}.{body}`,
  keyed by the secret with `whsec_` stripped and the rest base64-decoded. The `svix-signature`
  header may hold several space-separated `v1,...` entries during a secret rotation; any match
  passes.
- The pinned Resend .NET SDK (0.2.1) has no receiving surface, which is why the read is a plain
  HTTP call while sending still goes through the SDK.

## Where the code lives

See the **Broker rate negotiation (email)** entry in
[.claude/feature-map.md](../.claude/feature-map.md) for the file-by-file map.

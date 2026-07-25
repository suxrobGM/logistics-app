# MCP Client (outbound integrations)

- **Status**: Planned
- **Priority**: P2 - we're the only TMS that's an MCP _server_; becoming an MCP _client_ makes our agent the hub of the carrier's AI stack
- **Effort**: L
- **Category**: AI differentiation / integrations strategy

## Why

Alvys markets 120+ integrations built as EDI/API projects. MCP inverts this: a tenant connects any
external MCP server (fuel card provider, customer's WMS, factoring service, weather/routing services)
and our dispatch agent can call its tools - integrations become configuration, not engineering projects.
Nobody in the TMS space can tell this story.

## What to build

- Tenant-configurable external MCP connections: URL + auth (API key / OAuth), stored per tenant with encrypted secrets; management UI under `tms-portal/pages/settings/`.
- MCP client in `Infrastructure.AI`: discover remote tools, merge into the session's tool list alongside `AIDispatchToolRegistry` (namespaced, e.g. `ext_fuelcard_get_balance`).
- **Security is the hard part**: external tools are untrusted - wrap results in data-only framing to resist prompt injection, cap result sizes, and route any external _write_ tool through the decision approval flow unconditionally. Per-connection allowlist of which tools the agent may see.
- Marketplace angle later: curated directory of verified logistics MCP servers; also publish our own MCP server to public directories (zero-effort distribution).
- Admin kill switch per tenant (`TenantFeature` flag, Enterprise plan).

## Acceptance

A tenant connects a demo external MCP server; the dispatch agent discovers and calls one of its read tools mid-session, with the call visible in the session transcript; external write tools always require approval.

## Notes

_(add dated implementation notes here)_
